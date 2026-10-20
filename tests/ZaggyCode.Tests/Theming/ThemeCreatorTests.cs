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
        actualMetadata.WindowBackgroundColor.Should().Be(_metadata.WindowBackgroundColor);
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
        actualTheme!.WindowBackgroundColor.Should().Be(_theme.WindowBackgroundColor);
        actualTheme.MapWallColor.Should().Be(_theme.MapWallColor);
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }

    private Theme CreateTheme() => new()
    {
        WindowBackgroundColor = "#101010",
        HeaderBackgroundColor = "#202020",
        CardBackgroundColor = "#303030",
        ElevatedBackgroundColor = "#404040",
        HoverBackgroundColor = "#505050",
        AccentColor = "#606060",
        AccentHoverColor = "#707070",
        AccentPressedColor = "#808080",
        TextPrimaryColor = "#909090",
        TextMutedColor = "#A0A0A0",
        TextSubtleColor = "#B0B0B0",
        BorderColor = "#C0C0C0",
        BorderHighlightColor = "#D0D0D0",
        SuccessColor = "#E0E0E0",
        SuccessHoverColor = "#F0F0F0",
        ErrorColor = "#010203",
        WarningColor = "#040506",
        EditorBackgroundColor = "#070809",
        EditorForegroundColor = "#0A0B0C",
        EditorLineNumberColor = "#0D0E0F",
        TerminalBackgroundColor = "#111213",
        TerminalForegroundColor = "#141516",
        SidebarBackgroundColor = "#171819",
        SystemAccentColor = "#1A1B1C",
        MapWallColor = _fixture.Create<string>(),
        MapPointBorderColor = _fixture.Create<string>(),
        MapBackgroundColor = _fixture.Create<string>(),
        MapPaintedCellColor = _fixture.Create<string>(),
        MapRequireDrawColor = _fixture.Create<string>(),
        GoalColor1 = _fixture.Create<string>(),
        GoalColor2 = _fixture.Create<string>()
    };

    private ThemeMetadata CreateMetadata() => new()
    {
        Name = _fixture.Create<string>(),
        Description = _fixture.Create<string>(),
        CreatedAtVersion = new Version(1, 0),
        Author = _fixture.Create<string>(),
        WindowBackgroundColor = "#101010",
        SidebarBackgroundColor = "#171819",
        EditorBackgroundColor = "#070809",
        TerminalBackgroundColor = "#111213",
        AccentColor = "#606060",
        BorderColor = "#C0C0C0",
        TextPrimaryColor = "#909090"
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
