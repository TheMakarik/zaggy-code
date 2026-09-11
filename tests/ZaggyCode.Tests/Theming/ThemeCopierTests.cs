namespace ZaggyCode.Tests.Theming;

using System.Text.Json;

public class ThemeCopierTests : IDisposable
{
    private readonly TestFileSystem _fileSystem = new();
    private readonly IOptions<ThemeOptions> _themeOptions;
    private readonly IOptions<MetadataOptions> _metadataOptions;
    private readonly IOptions<TempOptions> _tempOptions;
    private readonly TarBZip2ArchiveReader _reader;
    private readonly TarBZip2ArchiveCompressor _compressor;

    public ThemeCopierTests()
    {
        var metadataOptions = new MetadataOptions { MetadataFile = "meta.json" };
        _metadataOptions = A.Fake<IOptions<MetadataOptions>>();
        A.CallTo(() => _metadataOptions.Value).Returns(metadataOptions);

        _themeOptions = A.Fake<IOptions<ThemeOptions>>();
        A.CallTo(() => _themeOptions.Value).Returns(new ThemeOptions
        {
            ThemeExtensions = ".zct",
            ThemeFileName = "theme.xml",
            SystemThemesFolder = _fileSystem.CreateDirectory(),
            ExternThemesFolder = _fileSystem.CreateDirectory()
        });

        _tempOptions = CreateTempOptions();
        _reader = new TarBZip2ArchiveReader(
            A.Dummy<ILogger<TarBZip2ArchiveReader>>(),
            new MetadataParser(_metadataOptions),
            _tempOptions);
        _compressor = new TarBZip2ArchiveCompressor(
            A.Dummy<ILogger<TarBZip2ArchiveCompressor>>(),
            _tempOptions);
    }

    [Fact]
    public async Task CopyThemeAsync_WhenSourceExists_CopiesToExternFolderWithDistinctName()
    {
        // Arrange
        var sourcePath = await CreateArchiveAsync("Primus", _themeOptions.Value.SystemThemesFolder);
        var theme = CreateMetadata("Primus", sourcePath);
        var systemUnderTests = CreateSystemUnderTests();

        // Act
        var copiedPath = await systemUnderTests.CopyThemeAsync(theme);

        // Assert
        copiedPath.Should().Be(Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия.zct"));
        var copiedMetadata = await _reader.ReadMetadataAsync<ThemeMetadata>(copiedPath);
        File.Exists(copiedPath).Should().BeTrue();
        copiedMetadata!.Name.Should().Be("Primus - копия");
    }

    [Fact]
    public async Task CopyThemeAsync_WhenCopyNameAlreadyExists_AddsSequentialNumber()
    {
        // Arrange
        var sourcePath = await CreateArchiveAsync("Primus", _themeOptions.Value.SystemThemesFolder);
        await CreateArchiveAsync("Primus - копия", _themeOptions.Value.ExternThemesFolder);
        var theme = CreateMetadata("Primus", sourcePath);
        var systemUnderTests = CreateSystemUnderTests();

        // Act
        var copiedPath = await systemUnderTests.CopyThemeAsync(theme);

        // Assert
        copiedPath.Should().Be(Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия (1).zct"));
        var copiedMetadata = await _reader.ReadMetadataAsync<ThemeMetadata>(copiedPath);
        copiedMetadata!.Name.Should().Be("Primus - копия (1)");
    }

    [Fact]
    public async Task CopyThemeAsync_WhenMultipleCopiesExist_AddsNextNumber()
    {
        // Arrange
        var sourcePath = await CreateArchiveAsync("Primus", _themeOptions.Value.SystemThemesFolder);
        await CreateArchiveAsync("Primus - копия", _themeOptions.Value.ExternThemesFolder);
        await CreateArchiveAsync("Primus - копия (1)", _themeOptions.Value.ExternThemesFolder);
        var theme = CreateMetadata("Primus", sourcePath);
        var systemUnderTests = CreateSystemUnderTests();

        // Act
        var copiedPath = await systemUnderTests.CopyThemeAsync(theme);

        // Assert
        copiedPath.Should().Be(Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия (2).zct"));
    }

    [Fact]
    public async Task CopyThemeAsync_WhenSourcePathMissing_ThrowsFileNotFoundException()
    {
        // Arrange
        var theme = CreateMetadata("Primus", Path.Join(_fileSystem.CreateDirectory(), "missing.zct"));
        var systemUnderTests = CreateSystemUnderTests();

        // Act
        var act = async () => await systemUnderTests.CopyThemeAsync(theme);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    private ThemeCopier CreateSystemUnderTests()
    {
        return new ThemeCopier(
            A.Dummy<ILogger<ThemeCopier>>(),
            _reader,
            _compressor,
            new TempFolderProvider(_tempOptions),
            _themeOptions,
            _metadataOptions);
    }

    private async Task<string> CreateArchiveAsync(string name, string folder)
    {
        var stagingDirectory = Path.Join(_tempOptions.Value.TempToCompress, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingDirectory);

        await using (var metadataStream = File.Create(Path.Join(stagingDirectory, "meta.json")))
        {
            await JsonSerializer.SerializeAsync(metadataStream, CreateMetadata(name, string.Empty));
        }

        await File.WriteAllTextAsync(Path.Join(stagingDirectory, "theme.xml"), "<theme/>");

        var archivePath = Path.Join(folder, $"{name}.zct");
        await _compressor.CompressAsync(archivePath, stagingDirectory);
        return archivePath;
    }

    private static ThemeMetadata CreateMetadata(string name, string path) => new()
    {
        Name = name,
        CreatedAtVersion = new Version(1, 0),
        Author = "Author",
        BackgroundColor = "#101010",
        SidebarBackgroundColor = "#111111",
        EditorBackgroundColor = "#121212",
        TerminalBackgroundColor = "#131313",
        PrimaryColor = "#141414",
        BorderColor = "#151515",
        ForegroundColor = "#161616",
        Path = path
    };

    private IOptions<TempOptions> CreateTempOptions()    {
        var tempBase = _fileSystem.CreateDirectory();
        var options = new TempOptions
        {
            TempDirectoryPath = Path.Join(tempBase, "temp"),
            TempToCompress = Path.Join(tempBase, "to-compress"),
            TempFromCompress = Path.Join(tempBase, "from-compress")
        };

        var fake = A.Fake<IOptions<TempOptions>>();
        A.CallTo(() => fake.Value).Returns(options);
        return fake;
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}
