using System.ComponentModel;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheMakarik.Testing.FileSystem;
using TheMakarik.Testing.FileSystem.AutoNaming;
using TheMakarik.Testing.FileSystem.Core;
using ZaggyCode.Data.Interfaces;
using ZaggyCode.Data.Model;
using ZaggyCode.Data.Options;
using ZaggyCode.Languages.Enums;

namespace ZaggyCode.Data.Tests;

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
                TerminalFontSize = 17
            }
        });
        A.CallTo(() => _stubProvider.GetFolder(An<Environment.SpecialFolder>.Ignored, _jsonPath)).Returns(_jsonPath);
    }

    [Fact]
    public async Task UserProperty_AfterFlush_UpdateUserDataForce()
    {
        //Arrange
        ILogger<UserStorage> logger = A.Dummy<ILogger<UserStorage>>();
        IOptions<StorageOptions> options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });
        UserStorage systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
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
        ILogger<UserStorage> logger = A.Dummy<ILogger<UserStorage>>();
        IOptions<StorageOptions> options = A.Fake<IOptions<StorageOptions>>();

        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });

        await File.WriteAllTextAsync(_jsonPath, "{ invalid: json }");
        var corruptedContent = await File.ReadAllTextAsync(_jsonPath);
        UserData expectedUser = _userDefaultMock.Value.User;

        UserStorage systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);

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
        ILogger<UserStorage> logger = A.Dummy<ILogger<UserStorage>>();
        IOptions<StorageOptions> options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 1,
            GameCodeDataPath = _stubDirectoryPath
        });

        UserStorage systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
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
        ILogger<UserStorage> logger = A.Dummy<ILogger<UserStorage>>();
        IOptions<StorageOptions> options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });

        UserStorage systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
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
        ILogger<UserStorage> logger = A.Dummy<ILogger<UserStorage>>();
        IOptions<StorageOptions> options = A.Fake<IOptions<StorageOptions>>();
        A.CallTo(() => options.Value).Returns(new StorageOptions()
        {
            DataFilePath = _jsonPath,
            WaitUserDataUpdateSeconds = 3,
            GameCodeDataPath = _stubDirectoryPath
        });

        UserStorage systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);
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