namespace ZaggyCode.Tests.Theming;

public class ThemeCopierTests : IDisposable
{
    private readonly TestFileSystem _fileSystem = new();
    private readonly IOptions<ThemeOptions> _themeOptions;

    public ThemeCopierTests()
    {
        _themeOptions = A.Fake<IOptions<ThemeOptions>>();
        A.CallTo(() => _themeOptions.Value).Returns(new ThemeOptions
        {
            ThemeExtensions = ".zct",
            ThemeFileName = "theme.xml",
            SystemThemesFolder = _fileSystem.CreateDirectory(),
            ExternThemesFolder = _fileSystem.CreateDirectory()
        });
    }

    [Fact]
    public async Task CopyThemeAsync_WhenSourceExists_CopiesToExternFolderAndReturnsPath()
    {
        // Arrange
        var sourcePath = _fileSystem.CreateFile(".zct");
        var sourceContent = "archive-content";
        File.WriteAllText(sourcePath, sourceContent);
        var theme = CreateMetadata("Primus", sourcePath);
        var systemUnderTests = CreateSystemUnderTests();

        // Act
        var copiedPath = await systemUnderTests.CopyThemeAsync(theme);

        // Assert
        copiedPath.Should().Be(Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия.zct"));
        File.Exists(copiedPath).Should().BeTrue();
        File.ReadAllText(copiedPath).Should().Be(sourceContent);
    }

    [Fact]
    public async Task CopyThemeAsync_WhenCopyNameAlreadyExists_AddsSequentialNumber()
    {
        // Arrange
        var sourcePath = _fileSystem.CreateFile(".zct");
        var existingCopy = Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия.zct");
        File.WriteAllText(existingCopy, "existing");
        var theme = CreateMetadata("Primus", sourcePath);
        var systemUnderTests = CreateSystemUnderTests();

        // Act
        var copiedPath = await systemUnderTests.CopyThemeAsync(theme);

        // Assert
        copiedPath.Should().Be(Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия (1).zct"));
        File.Exists(copiedPath).Should().BeTrue();
    }

    [Fact]
    public async Task CopyThemeAsync_WhenMultipleCopiesExist_AddsNextNumber()
    {
        // Arrange
        var sourcePath = _fileSystem.CreateFile(".zct");
        File.WriteAllText(Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия.zct"), "a");
        File.WriteAllText(Path.Join(_themeOptions.Value.ExternThemesFolder, "Primus - копия (1).zct"), "b");
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
        return new ThemeCopier(A.Dummy<ILogger<ThemeCopier>>(), _themeOptions);
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

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}
