namespace ZaggyCode.Tests.Data;

public class UserStorageTests : IDisposable
{
    private readonly TestFileSystem _fileSystem;
    private readonly string _jsonPath;
    private readonly string _stubDirectoryPath;
    private readonly ISpecialFolderProvider _stubProvider = A.Fake<ISpecialFolderProvider>();
    private readonly IOptions<DefaultUser> _userDefaultMock;

    public UserStorageTests()
    {
        _fileSystem = new TestFileSystem();
        _jsonPath = _fileSystem.CreateFile(".json");
        _stubDirectoryPath = _fileSystem.CreateDirectory();

        _userDefaultMock = A.Fake<IOptions<DefaultUser>>();
        A.CallTo(() => _userDefaultMock.Value).Returns(new DefaultUser()
        {
            User = new UserData()
            {
                EnableCodeHighlighting = true,
                ShowCodeLineNumbers = true,
                CodeFontSize = 14,
                CodeTheme = "Light",
                LastLanguage = Language.CSharp,
                LastGamePath = null,
                LastSpeed = ExecutionSpeed.X2,
                TerminalFontSize = 17
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
        var actualContent = await File.ReadAllTextAsync(_jsonPath);
        actualContent.Should().NotBe(firstContent);
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
        var actualContent = await File.ReadAllTextAsync(_jsonPath);
        actualContent.Should().Contain(expectedUser.CodeFontSize.ToString());
        actualContent.Should().Contain(expectedUser.EnableCodeHighlighting.ToString().ToLower());
        actualContent.Should().NotBe(corruptedContent);
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
        var actualContent = await File.ReadAllTextAsync(_jsonPath);
        actualContent.Should().Contain(newTheme);
        actualContent.Should().Contain(newLineNumbers.ToString().ToLower());
        actualContent.Should().NotBe(firstContent);
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
        var actualContent = await File.ReadAllTextAsync(_jsonPath);
        actualContent.Should().Contain(newFontSize.ToString());
        actualContent.Should().NotBe(firstContent);
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
        var actualContent = await File.ReadAllTextAsync(_jsonPath);
        actualContent.Should().Contain(expectedFontSize.ToString());
        actualContent.Should().Contain(expectedTheme);
        actualContent.Should().NotBe(originalContent);
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}
