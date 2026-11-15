namespace ZaggyCode.Tests.Theming;

public class ThemeEditorServiceTests : IDisposable
{
    private const string ArchiveExtension = ".zct";

    private readonly TestFileSystem _fileSystem = new();
    private readonly string _systemFolder;
    private readonly string _externFolder;
    private readonly ThemeCatalog _catalog;
    private readonly ThemeCreator _creator;
    private readonly ThemeEditorService _systemUnderTests;

    public ThemeEditorServiceTests()
    {
        _systemFolder = _fileSystem.CreateDirectory();
        _externFolder = _fileSystem.CreateDirectory();
        var themeOptions = CreateThemeOptions();
        var metadataOptions = CreateMetadataOptions();
        var tempOptions = CreateTempOptions();
        var reader = new TarBZip2ArchiveReader(
            A.Dummy<ILogger<TarBZip2ArchiveReader>>(),
            new MetadataParser(metadataOptions, A.Dummy<ILogger<MetadataParser>>()),
            new MetadataCache(A.Dummy<ILogger<IMetadataCache>>()),
            tempOptions);

        _catalog = new ThemeCatalog(
            A.Dummy<ILogger<ThemeCatalog>>(),
            reader,
            themeOptions,
            new XmlSerializer<Theme>());
        _creator = new ThemeCreator(
            A.Dummy<ILogger<ThemeCreator>>(),
            new TarBZip2ArchiveCompressor(A.Dummy<ILogger<TarBZip2ArchiveCompressor>>(), tempOptions),
            new TempFolderProvider(tempOptions),
            themeOptions,
            metadataOptions,
            new XmlSerializer<Theme>());
        _systemUnderTests = new ThemeEditorService(
            A.Dummy<ILogger<ThemeEditorService>>(),
            _catalog,
            _creator,
            A.Dummy<IThemeSetterProxy>(),
            themeOptions,
            A.Dummy<IOptions<ThemeColorDisplayNameOptions>>());
    }

    [Fact]
    public async Task SaveAsync_WhenExternalThemeEdited_ChangesAreLoadedBack()
    {
        // Arrange
        await CreateThemeArchiveAsync("Custom", "#101010", _externFolder);

        await _systemUnderTests.BeginEditAsync("Custom");
        _systemUnderTests.SetColor(nameof(Theme.WindowBackgroundColor), "#ABCDEF");

        // Act
        await _systemUnderTests.SaveAsync();
        var theme = await _catalog.LoadThemeAsync("Custom");

        // Assert
        theme.Should().NotBeNull();
        theme!.WindowBackgroundColor.Should().Be("#ABCDEF");
    }

    [Fact]
    public async Task SaveAsync_WhenExternalThemeShadowsSystemTheme_ChangesAreLoadedBack()
    {
        // Arrange
        await CreateThemeArchiveAsync("Primus", "#101010", _systemFolder);
        await CreateThemeArchiveAsync("Primus", "#202020", _externFolder);

        await _systemUnderTests.BeginEditAsync("Primus");
        _systemUnderTests.SetColor(nameof(Theme.WindowBackgroundColor), "#ABCDEF");

        // Act
        await _systemUnderTests.SaveAsync();
        var theme = await _catalog.LoadThemeAsync("Primus");

        // Assert
        theme.Should().NotBeNull();
        theme!.WindowBackgroundColor.Should().Be("#ABCDEF");
    }

    private Task CreateThemeArchiveAsync(string name, string background, string directory)
    {
        var metadata = new ThemeMetadata
        {
            Name = name,
            Description = name,
            CreatedAtVersion = new Version(1, 0),
            Author = "Author",
            WindowBackgroundColor = background,
            SidebarBackgroundColor = background,
            EditorBackgroundColor = background,
            TerminalBackgroundColor = background,
            AccentColor = background,
            BorderColor = background,
            TextPrimaryColor = background
        };

        return _creator.CreateAsync(CreateTheme(background), metadata, directory);
    }

    private static Theme CreateTheme(string background) => new()
    {
        WindowBackgroundColor = background,
        HeaderBackgroundColor = background,
        DescriptionBackgroundColor = background,
        SearchBackgroundColor = background,
        CardBackgroundColor = background,
        ElevatedBackgroundColor = background,
        HoverBackgroundColor = background,
        AccentColor = background,
        AccentHoverColor = background,
        AccentPressedColor = background,
        TextPrimaryColor = background,
        TextMutedColor = background,
        TextSubtleColor = background,
        BorderColor = background,
        BorderHighlightColor = background,
        SuccessColor = background,
        SuccessHoverColor = background,
        ErrorColor = background,
        WarningColor = background,
        EditorBackgroundColor = background,
        EditorForegroundColor = background,
        EditorLineNumberColor = background,
        TerminalBackgroundColor = background,
        TerminalForegroundColor = background,
        SidebarBackgroundColor = background,
        SystemAccentColor = background,
        MapWallColor = background,
        MapPointBorderColor = background,
        MapBackgroundColor = background,
        MapPaintedCellColor = background,
        MapRequireDrawColor = background,
        GoalColor1 = background,
        GoalColor2 = background
    };

    private IOptions<ThemeOptions> CreateThemeOptions()
    {
        var options = A.Fake<IOptions<ThemeOptions>>();
        A.CallTo(() => options.Value).Returns(new ThemeOptions
        {
            ThemeExtensions = ArchiveExtension,
            ThemeFileName = "theme.xml",
            SystemThemesFolder = _systemFolder,
            ExternThemesFolder = _externFolder
        });
        return options;
    }

    private IOptions<MetadataOptions> CreateMetadataOptions()
    {
        var options = A.Fake<IOptions<MetadataOptions>>();
        A.CallTo(() => options.Value).Returns(new MetadataOptions { MetadataFile = "meta.json" });
        return options;
    }

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

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}
