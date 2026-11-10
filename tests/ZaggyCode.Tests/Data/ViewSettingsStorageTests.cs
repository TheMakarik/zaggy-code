using ZaggyCode.Modules.Data.Json;

namespace ZaggyCode.Tests.Data;

public class ViewSettingsStorageTests : IDisposable
{
    private readonly TestFileSystem _fileSystem;
    private readonly string _jsonPath;
    private readonly string _stubDirectoryPath;
    private readonly IOptions<DefaultViewSettings> _viewDefaultMock;

    public ViewSettingsStorageTests()
    {
        _fileSystem = new TestFileSystem();
        _jsonPath = _fileSystem.CreateFile(".json");
        _stubDirectoryPath = _fileSystem.CreateDirectory();

        _viewDefaultMock = A.Fake<IOptions<DefaultViewSettings>>();
        A.CallTo(() => _viewDefaultMock.Value).Returns(new DefaultViewSettings
        {
            Settings = new ViewSettings
            {
                SidebarPosition = SidebarPosition.Right,
                CodeFontSize = 14,
                CodeTheme = "Light",
                TerminalFontSize = 17
            }
        });
    }

    private ObservableStorage<ViewSettings> CreateSystemUnderTests(int waitSettingsUpdateSeconds = 3)
    {
        var logger = A.Dummy<ILogger<ObservableStorage<ViewSettings>>>();
        var updateWaiter = new UpdateStorageWaiter(A.Dummy<ILogger<UpdateStorageWaiter>>());

        return new ObservableStorage<ViewSettings>(
            logger,
            _jsonPath,
            TimeSpan.FromSeconds(waitSettingsUpdateSeconds),
            ViewSettingsSerializerContext.Default.ViewSettings,
            updateWaiter,
            () => _viewDefaultMock.Value.Settings);
    }

    [Fact]
    public async Task ViewProperty_AfterFlush_UpdateViewSettingsForce()
    {
        //Arrange
        var systemUnderTests = CreateSystemUnderTests();
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
        await File.WriteAllTextAsync(_jsonPath, "{ invalid: json }");
        var corruptedContent = await File.ReadAllTextAsync(_jsonPath);
        var expectedViewSettings = _viewDefaultMock.Value.Settings;

        var systemUnderTests = CreateSystemUnderTests();

        // Act
        await systemUnderTests.LoadAsync();

        // Assert
        var actualContent = await File.ReadAllTextAsync(_jsonPath);
        actualContent.Should().Contain(expectedViewSettings.CodeFontSize.ToString());
        actualContent.Should().Contain(expectedViewSettings.CodeTheme);
        actualContent.Should().NotBe(corruptedContent);
    }

    [Fact]
    public async Task BeginObserve_WhenPropertyChanged_AutoSavesAfterDelay()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests(1);
        await systemUnderTests.LoadAsync();

        var firstContent = await File.ReadAllTextAsync(_jsonPath);
        var newTheme = "Dark";
        var newSidebarPosition = SidebarPosition.Hidden;

        // Act
        systemUnderTests.Current.CodeTheme = newTheme;
        systemUnderTests.Current.SidebarPosition = newSidebarPosition;

        await Task.Delay(1500);

        // Assert
        var actualContent = await File.ReadAllTextAsync(_jsonPath);
        actualContent.Should().Contain(newTheme);
        actualContent.Should().Contain(((int)newSidebarPosition).ToString());
        actualContent.Should().NotBe(firstContent);
    }

    [Fact]
    public async Task LoadAsync_CalledTwice_DoesNotDuplicateObservers()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();

        var firstContent = await File.ReadAllTextAsync(_jsonPath);
        var newFontSize = _viewDefaultMock.Value.Settings.CodeFontSize + 10;

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
        var systemUnderTests = CreateSystemUnderTests();
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
