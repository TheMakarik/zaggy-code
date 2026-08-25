namespace ZaggyCode.Tests.Theming;

public class ThemeCreatorTests : IDisposable
{
    private const string ArchiveExtension = ".zct";
    private const string ThemeFileName = "theme.xml";

    private readonly Fixture _fixture = new();
    private readonly TestFileSystem _fileSystem = new();
    private readonly ThemeCreator _systemUnderTests;
    private readonly TarBZip2ArchiveReader _reader;
    private readonly Theme _theme;
    private readonly ThemeMetadata _metadata;

    public ThemeCreatorTests()
    {
        var options = new MetadataOptions() { MetadataFile = "meta.json" };
        var fakeOptions = A.Fake<IOptions<MetadataOptions>>();
        A.CallTo(() => fakeOptions.Value).Returns(options);
        var tempOptions = CreateTempOptions();
        _systemUnderTests = new ThemeCreator(
            A.Dummy<ILogger<ThemeCreator>>(),
            new TarBZip2ArchiveCompressor(A.Dummy<ILogger<TarBZip2ArchiveCompressor>>(), tempOptions),
            new TempFolderProvider(tempOptions),
            CreateThemeOptions(),
            fakeOptions,
            new XmlSerializer<Theme>());
        _reader = new TarBZip2ArchiveReader(
            A.Dummy<ILogger<TarBZip2ArchiveReader>>(),
            new MetadataParser(fakeOptions),
            tempOptions);

        _theme = CreateTheme();
        _metadata = CreateMetadata();
    }

    [Fact]
    public async Task CreateAsync_WhenThemeProvided_CreatesArchiveInOutputDirectory()
    {
        // Arrange
        var outputDirectory = _fileSystem.CreateDirectory();

        // Act
        await _systemUnderTests.CreateAsync(_theme, _metadata, outputDirectory);

        // Assert
        File.Exists(Path.Join(outputDirectory, _metadata.Name + ArchiveExtension)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WhenArchiveCreated_MetadataIsReadableAsJson()
    {
        // Arrange
        var outputDirectory = _fileSystem.CreateDirectory();
        await _systemUnderTests.CreateAsync(_theme, _metadata, outputDirectory);
        var archivePath = Path.Join(outputDirectory, _metadata.Name + ArchiveExtension);

        // Act
        var actualMetadata = await _reader.ReadMetadataAsync<ThemeMetadata>(archivePath);

        // Assert
        actualMetadata.Should().NotBeNull();
        actualMetadata!.Name.Should().Be(_metadata.Name);
        actualMetadata.Author.Should().Be(_metadata.Author);
        actualMetadata.CreatedAtVersion.Should().Be(_metadata.CreatedAtVersion);
        actualMetadata.BackgroundColor.Should().Be(_metadata.BackgroundColor);
    }

    [Fact]
    public async Task CreateAsync_WhenArchiveCreated_ContainsDeserializableTheme()
    {
        // Arrange
        var outputDirectory = _fileSystem.CreateDirectory();
        await _systemUnderTests.CreateAsync(_theme, _metadata, outputDirectory);
        var archivePath = Path.Join(outputDirectory, _metadata.Name + ArchiveExtension);

        // Act
        var extractedDirectory = await _reader.ExtractAllToTempAsync(archivePath, new Progress<int>());
        var themeFile = Path.Join(extractedDirectory.FullName, ThemeFileName);

        // Assert
        File.Exists(themeFile).Should().BeTrue();
        await using var themeStream = File.OpenRead(themeFile);
        var actualTheme = new XmlSerializer<Theme>().Deserialize(themeStream);
        actualTheme.Should().NotBeNull();
        actualTheme!.BackgroundColor.Should().Be(_theme.BackgroundColor);
        actualTheme.MapWallColor.Should().Be(_theme.MapWallColor);
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }

    private Theme CreateTheme() => new()
    {
        BackgroundColor = "#101010",
        BackgroundSecondaryColor = "#202020",
        SurfaceColor = "#303030",
        SurfaceLightColor = "#404040",
        SurfaceHoverColor = "#505050",
        PrimaryColor = "#606060",
        PrimaryLightColor = "#707070",
        PrimaryDarkColor = "#808080",
        ForegroundColor = "#909090",
        ForegroundMutedColor = "#A0A0A0",
        ForegroundDarkColor = "#B0B0B0",
        BorderColor = "#C0C0C0",
        BorderLightColor = "#D0D0D0",
        SuccessColor = "#E0E0E0",
        SuccessDarkColor = "#F0F0F0",
        ErrorColor = "#010203",
        WarningColor = "#040506",
        EditorBackgroundColor = "#070809",
        EditorForegroundColor = "#0A0B0C",
        EditorLineNumberColor = "#0D0E0F",
        TerminalBackgroundColor = "#111213",
        TerminalForegroundColor = "#141516",
        SidebarBackgroundColor = "#171819",
        SystemAccentColor = "#1A1B1C",
        SystemAccentColorLight1 = "#1D1E1F",
        SystemAccentColorDark1 = "#202122",
        MapWallColor = _fixture.Create<string>(),
        MapPointBorderColor = _fixture.Create<string>(),
        MapPointBackgroundColor = _fixture.Create<string>()
    };

    private ThemeMetadata CreateMetadata() => new()
    {
        Name = _fixture.Create<string>(),
        Description = _fixture.Create<string>(),
        CreatedAtVersion = new Version(1, 0),
        Author = _fixture.Create<string>(),
        BackgroundColor = "#101010",
        SidebarBackgroundColor = "#171819",
        EditorBackgroundColor = "#070809",
        TerminalBackgroundColor = "#111213",
        PrimaryColor = "#606060",
        BorderColor = "#C0C0C0",
        ForegroundColor = "#909090"
    };

    private IOptions<TempOptions> CreateTempOptions()
    {
        var options = A.Fake<IOptions<TempOptions>>();
        A.CallTo(() => options.Value).Returns(new TempOptions
        {
            TempDirectoryPath = _fileSystem.CreateDirectory(),
            TempToCompress = _fileSystem.CreateDirectory(),
            TempFromCompress = _fileSystem.CreateDirectory()
        });
        return options;
    }

    private IOptions<ThemeOptions> CreateThemeOptions()
    {
        var options = A.Fake<IOptions<ThemeOptions>>();
        A.CallTo(() => options.Value).Returns(new ThemeOptions
        {
            ThemeExtensions = ArchiveExtension,
            ThemeFileName = "theme.xml",
            SystemThemesFolder = _fileSystem.CreateDirectory(),
            ExternThemesFolder = _fileSystem.CreateDirectory()
        });
        return options;
    }
}
