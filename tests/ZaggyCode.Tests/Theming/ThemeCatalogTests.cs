namespace ZaggyCode.Tests.Theming;

public class ThemeCatalogTests : IDisposable
{
    private readonly TestFileSystem _fileSystem = new();
    private readonly IOptions<ThemeOptions> _themeOptions;
    private readonly string _systemFolder;
    private readonly string _externFolder;

    public ThemeCatalogTests()
    {
        _systemFolder = _fileSystem.CreateDirectory();
        _externFolder = _fileSystem.CreateDirectory();
        _themeOptions = A.Fake<IOptions<ThemeOptions>>();
        A.CallTo(() => _themeOptions.Value).Returns(new ThemeOptions
        {
            ThemeExtensions = ".zct",
            ThemeFileName = "theme.xml",
            SystemThemesFolder = _systemFolder,
            ExternThemesFolder = _externFolder
        });
    }

    [Fact]
    public async Task GetAvailableThemesAsync_WhenThemeFromSystemFolder_MarksAsSystemTheme()
    {
        // Arrange
        var systemFile = Path.Join(_systemFolder, "Primus.zct");
        File.WriteAllText(systemFile, "x");
        var reader = A.Fake<IArchiveReader>();
        A.CallTo(() => reader.ReadMetadataAsync<ThemeMetadata>(systemFile))
            .Returns(Task.FromResult<ThemeMetadata?>(CreateMetadata("Primus", "#100000")));
        var systemUnderTests = CreateSystemUnderTests(reader);

        // Act
        var themes = await systemUnderTests.GetAvailableThemesAsync();

        // Assert
        themes.Should().ContainSingle();
        themes[0].IsSystemTheme.Should().BeTrue();
        themes[0].Path.Should().Be(systemFile);
    }

    [Fact]
    public async Task GetAvailableThemesAsync_WhenThemeFromExternFolder_MarksAsNotSystem()
    {
        // Arrange
        var externFile = Path.Join(_externFolder, "Primus.zct");
        File.WriteAllText(externFile, "x");
        var reader = A.Fake<IArchiveReader>();
        A.CallTo(() => reader.ReadMetadataAsync<ThemeMetadata>(externFile))
            .Returns(Task.FromResult<ThemeMetadata?>(CreateMetadata("Primus", "#100000")));
        var systemUnderTests = CreateSystemUnderTests(reader);

        // Act
        var themes = await systemUnderTests.GetAvailableThemesAsync();

        // Assert
        themes.Should().ContainSingle();
        themes[0].IsSystemTheme.Should().BeFalse();
        themes[0].Path.Should().Be(externFile);
    }

    [Fact]
    public async Task GetAvailableThemesAsync_WhenExternOverridesSystem_ExternalThemeWins()
    {
        // Arrange
        var systemFile = Path.Join(_systemFolder, "Primus.zct");
        var externFile = Path.Join(_externFolder, "Primus.zct");
        File.WriteAllText(systemFile, "x");
        File.WriteAllText(externFile, "y");
        var reader = A.Fake<IArchiveReader>();
        A.CallTo(() => reader.ReadMetadataAsync<ThemeMetadata>(systemFile))
            .Returns(Task.FromResult<ThemeMetadata?>(CreateMetadata("Primus", "#100000")));
        A.CallTo(() => reader.ReadMetadataAsync<ThemeMetadata>(externFile))
            .Returns(Task.FromResult<ThemeMetadata?>(CreateMetadata("Primus", "#200000")));
        var systemUnderTests = CreateSystemUnderTests(reader);

        // Act
        var themes = await systemUnderTests.GetAvailableThemesAsync();

        // Assert
        themes.Should().ContainSingle();
        themes[0].IsSystemTheme.Should().BeFalse();
        themes[0].BackgroundColor.Should().Be("#200000");
        themes[0].Path.Should().Be(externFile);
    }

    private ThemeCatalog CreateSystemUnderTests(IArchiveReader reader)
    {
        return new ThemeCatalog(
            A.Dummy<ILogger<ThemeCatalog>>(),
            reader,
            _themeOptions,
            A.Dummy<ITempFolderProvider>(),
            new XmlSerializer<Theme>());
    }

    private static ThemeMetadata CreateMetadata(string name, string background) => new()
    {
        Name = name,
        CreatedAtVersion = new Version(1, 0),
        Author = "Author",
        BackgroundColor = background,
        SidebarBackgroundColor = "#111111",
        EditorBackgroundColor = "#121212",
        TerminalBackgroundColor = "#131313",
        PrimaryColor = "#141414",
        BorderColor = "#151515",
        ForegroundColor = "#161616"
    };

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}
