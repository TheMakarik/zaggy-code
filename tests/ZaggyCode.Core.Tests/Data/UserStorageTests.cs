namespace ZaggyCode.Data.Tests.Data;

public class UserStorageTests : IDisposable
{
    private readonly IFileSystem _fileSystem;
    private string _jsonPath;
    private string _stubDirectoryPath;
    private ISpecialFolderProvider _stubProvider = A.Fake<ISpecialFolderProvider>();
    private readonly IOptions<DefaultUser> _userDefaultMock;

    public UserStorageTests()
    {
        _fileSystem = FileSystem.BeginBuilding()
            .AddRandomInTempRootName()
            .AddNameGenerator(NameGenerationType.RandomName)
            .AddFileWithNameGeneraing(".json", out _jsonPath)
            .AddDirectoryWithNameGenerating(out _stubDirectoryPath, (_, innerBuilder) => innerBuilder)
            .Build();

        _userDefaultMock = A.Fake<IOptions<DefaultUser>>();
        A.CallTo(() => _userDefaultMock.Value).Returns(new DefaultUser()
        {
            User = new UserData()
            {
                EnableCodeHighlighting = true,
                ShowCodeLineNumbers = true,
                CodeFontSize = 14,
                CodeTheme = "Light",
                LastLanguage = Language.Lua,
                LastGamePath = null,
                LastSpeed = ExecutionSpeed.X2,
                TerminalFontSize = 17,
                LuaData = A.Dummy<LuaData>()
            }
        });
        A.CallTo(() => _stubProvider.GetFolder(An<Environment.SpecialFolder>.Ignored, _jsonPath)).Returns(_jsonPath);
    }

    [Fact]
    public async Task UserProperty_AfterFlush_UpdateUserDataForce()
    {
        //Arrange
        var logger = A.Dummy<ILogger<UserStorage>>();
        var options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });
        var systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
        await systemUnderTests.LoadAsync();
        var firstContent = await File.ReadAllTextAsync(_jsonPath);

        //Act
        systemUnderTests.Current.CodeFontSize = 25;
        await systemUnderTests.FlushUpdatesAsync();

        //Assert
        _fileSystem.Should()
            .No
            .FileContentEquals(_jsonPath, firstContent);
    }

    [Fact]
    public async Task LoadAsync_WhenFileCorrupted_DeletesAndCreatesNewFile()
    {
        // Arrange
        var logger = A.Dummy<ILogger<UserStorage>>();
        var options = A.Fake<IOptions<StorageOptions>>();

        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });

        await File.WriteAllTextAsync(_jsonPath, "{ invalid: json }");
        var corruptedContent = await File.ReadAllTextAsync(_jsonPath);
        var expectedUser = _userDefaultMock.Value.User;

        var systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);

        // Act
        await systemUnderTests.LoadAsync();

        // Assert
        _fileSystem.Should()
            .FileContains(_jsonPath, expectedUser.CodeFontSize.ToString())
            .FileContains(_jsonPath, expectedUser.EnableCodeHighlighting.ToString().ToLower())
            .No.FileContentEquals(_jsonPath, corruptedContent);
    }

    [Fact]
    public async Task BeginObserve_WhenPropertyChanged_AutoSavesAfterDelay()
    {
        // Arrange
        var logger = A.Dummy<ILogger<UserStorage>>();
        var options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 1,
            GameCodeDataPath = _stubDirectoryPath
        });

        var systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
        await systemUnderTests.LoadAsync();

        var firstContent = await File.ReadAllTextAsync(_jsonPath);
        var newTheme = "Dark";
        var newLineNumbers = false;

        // Act
        systemUnderTests.Current.CodeTheme = newTheme;
        systemUnderTests.Current.ShowCodeLineNumbers = newLineNumbers;

        await Task.Delay(1500);

        // Assert
        _fileSystem.Should()
            .FileContains(_jsonPath, newTheme)
            .FileContains(_jsonPath, newLineNumbers.ToString().ToLower())
            .No.FileContentEquals(_jsonPath, firstContent);
    }


    [Fact]
    public async Task LoadAsync_CalledTwice_DoesNotDuplicateObservers()
    {
        // Arrange
        var logger = A.Dummy<ILogger<UserStorage>>();
        var options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });

        var systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
        await systemUnderTests.LoadAsync();

        var firstContent = await File.ReadAllTextAsync(_jsonPath);
        var newFontSize = _userDefaultMock.Value.User.CodeFontSize + 10;

        // Act 
        await systemUnderTests.LoadAsync();
        systemUnderTests.Current.CodeFontSize = newFontSize;
        await systemUnderTests.FlushUpdatesAsync();

        // Assert 
        _fileSystem.Should()
            .FileContains(_jsonPath, newFontSize.ToString())
            .No.FileContentEquals(_jsonPath, firstContent);
    }

    [Fact]
    public async Task FlushUpdatesAsync_AfterPropertyChange_WritesCorrectValuesToFile()
    {
        // Arrange
        var logger = A.Dummy<ILogger<UserStorage>>();
        var options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });

        var systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
        await systemUnderTests.LoadAsync();

        var originalContent = await File.ReadAllTextAsync(_jsonPath);
        var expectedFontSize = 42;
        var expectedTheme = "Monokai";

        // Act
        systemUnderTests.Current.CodeFontSize = expectedFontSize;
        systemUnderTests.Current.CodeTheme = expectedTheme;
        await systemUnderTests.FlushUpdatesAsync();

        // Assert
        _fileSystem.Should()
            .FileContains(_jsonPath, expectedFontSize.ToString())
            .FileContains(_jsonPath, expectedTheme)
            .No.FileContentEquals(_jsonPath, originalContent);
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}