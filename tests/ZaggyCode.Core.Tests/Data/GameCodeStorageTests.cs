namespace ZaggyCode.Data.Tests.Data;

public class GameCodeStorageTests : IDisposable
{
    private readonly IFileSystem _fileSystem;
    private string _gameCodeDataPath;
    private readonly IUserStorage _userStorageMock;
    private readonly IOptions<StorageOptions> _storageOptions;
    private readonly ILogger<GameCodeStorage> _logger;

    public GameCodeStorageTests()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddNameGenerator(NameGenerationType.RandomNameAndCount)
            .AddDirectoryWithNameGenerating(out _gameCodeDataPath, (_, innerBuilder) => innerBuilder)
            .Build();

        _userStorageMock = A.Fake<IUserStorage>();
        _logger = A.Dummy<ILogger<GameCodeStorage>>();
        _storageOptions = A.Fake<IOptions<StorageOptions>>();
        
        A.CallTo(() => _storageOptions.Value).Returns(new StorageOptions()
        {
            DataFilePath = "unused.json",
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _gameCodeDataPath
        });
    }

    [Theory]
    [InlineData("/path/to/game.exe", Language.CSharp, ".cs")]
    [InlineData("/path/to/game.exe", Language.Lua, ".lua")]
    [InlineData("/path/to/script.lua", Language.Lua, ".lua")]
    [InlineData("/path/to/game", Language.CSharp, ".cs")]
    public async Task AddGameCode_WhenCalled_CreatesFileWithCorrectExtension(string gamePath, Language language, string expectedExtension)
    {
        // Arrange
        var code = "test code content";
        var systemUnderTests = new GameCodeStorage(_userStorageMock, _logger, _storageOptions);
        
        // Act
        systemUnderTests.AddGameCode(gamePath, code, language);
        await systemUnderTests.FlushUpdatesAsync();
        
        // Assert
        var files = Directory.GetFiles(_gameCodeDataPath);
        files.Should().HaveCount(1);
        Path.GetExtension(files[0]).Should().Be(expectedExtension);
    }

    [Theory]
    [InlineData("/path/to/game1.exe", "/path/to/game2.exe", Language.CSharp, ".cs")]
    [InlineData("/path/to/game1.exe", "/path/to/game2.exe", Language.Lua, ".lua")]
    [InlineData("/path/to/script1.py", "/path/to/script2.ss", Language.ShardScript, ".py")]
    public async Task AddGameCode_MultipleGames_CreatesSeparateFilesWithCorrectExtensions(
        string gamePath1, string gamePath2, Language language, string expectedExtension)
    {
        // Arrange
        var code1 = "code for first game";
        var code2 = "code for second game";
        var systemUnderTests = new GameCodeStorage(_userStorageMock, _logger, _storageOptions);
        
        // Act
        systemUnderTests.AddGameCode(gamePath1, code1, language);
        systemUnderTests.AddGameCode(gamePath2, code2, language);
        await systemUnderTests.FlushUpdatesAsync();
        
        // Assert
        var files = Directory.GetFiles(_gameCodeDataPath);
        files.Should().HaveCount(2);
        
        foreach (var file in files)
        {
            Path.GetExtension(file).Should().Be(expectedExtension);
        }
        
        var contents = new List<string>();
        foreach (var file in files)
        {
            contents.Add(await File.ReadAllTextAsync(file));
        }
        
        contents.Should().Contain(code1);
        contents.Should().Contain(code2);
    }

    [Theory]
    [InlineData(Language.CSharp, ".cs")]
    [InlineData(Language.Lua, ".lua")]
    public async Task AddGameCode_DifferentLanguages_CreatesFilesWithRespectiveExtensions(Language language, string expectedExtension)
    {
        // Arrange
        var gamePath = "/path/to/game.exe";
        var code = $"code for {language}";
        var systemUnderTests = new GameCodeStorage(_userStorageMock, _logger, _storageOptions);
        
        // Act
        systemUnderTests.AddGameCode(gamePath, code, language);
        await systemUnderTests.FlushUpdatesAsync();
        
        // Assert
        var files = Directory.GetFiles(_gameCodeDataPath);
        files.Should().HaveCount(1);
        Path.GetExtension(files[0]).Should().Be(expectedExtension);
        
        var content = await File.ReadAllTextAsync(files[0]);
        content.Should().Be(code);
    }

    [Fact]
    public async Task AddGameCode_MultipleLanguagesForSameGame_CreatesSeparateFiles()
    {
        // Arrange
        var gamePath = "/path/to/game.exe";
        var systemUnderTests = new GameCodeStorage(_userStorageMock, _logger, _storageOptions);
        
        // Act
        systemUnderTests.AddGameCode(gamePath, "csharp code", Language.CSharp);
        systemUnderTests.AddGameCode(gamePath, "lua code", Language.Lua);
        systemUnderTests.AddGameCode(gamePath, "ShardScript code", Language.ShardScript);
        await systemUnderTests.FlushUpdatesAsync();
        
        // Assert
        var files = Directory.GetFiles(_gameCodeDataPath);
        files.Should().HaveCount(3);
        
        var extensions = files.Select(Path.GetExtension).ToList();
        extensions.Should().Contain(".cs");
        extensions.Should().Contain(".lua");
        
        var contents = new List<string>();
        foreach (var file in files)
        {
            contents.Add(await File.ReadAllTextAsync(file));
        }
        
        contents.Should().Contain("csharp code");
        contents.Should().Contain("lua code");
        contents.Should().Contain("python code");
    }

    [Fact]
    public async Task FlushUpdatesAsync_AfterMultipleAdds_CreatesAllFilesWithCorrectContent()
    {
        // Arrange
        var games = new[]
        {
            (Path: "/path/to/game1.exe", Lang: Language.CSharp, Code: "// Hello World\nConsole.WriteLine();"),
            (Path: "/path/to/game2.lua", Lang: Language.Lua, Code: "-- Lua script\nprint('hello')"),
            (Path: "/path/to/game4.exe", Lang: Language.CSharp, Code: "class Program {}"),
            (Path: "/path/to/game5.exe", Lang: Language.Lua, Code: "function test() end"),
        };
        
        var systemUnderTests = new GameCodeStorage(_userStorageMock, _logger, _storageOptions);
        
        // Act
        foreach (var game in games)
        {
            systemUnderTests.AddGameCode(game.Path, game.Code, game.Lang);
        }
        await systemUnderTests.FlushUpdatesAsync();
        
        // Assert
        var files = Directory.GetFiles(_gameCodeDataPath);
        files.Should().HaveCount(games.Length);
        
        var contents = new List<string>();
        foreach (var file in files)
        {
            contents.Add(await File.ReadAllTextAsync(file));
        }
        
        foreach (var game in games)
        {
            contents.Should().Contain(game.Code);
        }
        
        foreach (var file in files)
        {
            File.Exists(file).Should().BeTrue();
            new FileInfo(file).Length.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task AddGameCode_WithoutFlush_NoFilesCreated()
    {
        // Arrange
        var gamePath = "/path/to/game.exe";
        var language = Language.CSharp;
        var code = "code not yet flushed";
        var systemUnderTests = new GameCodeStorage(_userStorageMock, _logger, _storageOptions);
        
        // Act
        systemUnderTests.AddGameCode(gamePath, code, language);
        
        // Assert
        var files = Directory.GetFiles(_gameCodeDataPath);
        files.Should().BeEmpty();
    }

    [Fact]
    public async Task FlushUpdatesAsync_AfterMultipleAdds_FileContentMatchesExactly()
    {
        // Arrange
        var gamePath = "/path/to/game.exe";
        var language = Language.ShardScript;
        var expectedCode = "def main():\n    print('Hello')\n    return True";
        
        var systemUnderTests = new GameCodeStorage(_userStorageMock, _logger, _storageOptions);
        
        // Act
        systemUnderTests.AddGameCode(gamePath, expectedCode, language);
        await systemUnderTests.FlushUpdatesAsync();
        
        // Assert
        var files = Directory.GetFiles(_gameCodeDataPath);
        var actualCode = await File.ReadAllTextAsync(files[0]);
        actualCode.Should().Be(expectedCode);
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}