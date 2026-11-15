using System.Text.Json;
using ZaggyCode.Modules.Data.Json;

namespace ZaggyCode.Tests.Data;

public class ViewSettingsStorageTests : IDisposable
{
    private readonly TestFileSystem _fileSystem;
    private readonly string _jsonPath;
    private readonly IOptions<DefaultViewSettings> _viewDefaultMock;
    private readonly IBackgroundTaskHelper _backgroundTaskHelper;

    public ViewSettingsStorageTests()
    {
        _fileSystem = new TestFileSystem();
        _jsonPath = _fileSystem.CreateFile(".json");

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

        _backgroundTaskHelper = A.Fake<IBackgroundTaskHelper>();
        A.CallTo(() => _backgroundTaskHelper.StartBackground(A<BackgroundTaskStartDto>._))
            .Invokes((BackgroundTaskStartDto startDto) =>
                startDto.DelegateToStart(new Progress<BackgroundProgressMove>()));
    }

    private SettingsStorage<ViewSettings> CreateSystemUnderTests(double waitBeforeFlashSeconds = 3)
    {
        return new SettingsStorage<ViewSettings>(
            A.Dummy<ILogger<SettingsStorage<ViewSettings>>>(),
            _jsonPath,
            _backgroundTaskHelper,
            BackgroundTaskKind.FlushingViewSettings,
            TimeSpan.FromSeconds(waitBeforeFlashSeconds),
            ViewSettingsSerializerContext.Default.ViewSettings,
            () => _viewDefaultMock.Value.Settings);
    }

    [Fact]
    public async Task FlushAsync_AfterPropertyChange_WritesCorrectValuesToFile()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();
        var originalContent = await File.ReadAllTextAsync(_jsonPath);

        // Act
        systemUnderTests.Value.CodeFontSize = 42;
        systemUnderTests.Value.CodeTheme = "Monokai";
        await systemUnderTests.FlushAsync();

        // Assert
        var actual = await ReadFileAsync();
        actual.CodeFontSize.Should().Be(42);
        actual.CodeTheme.Should().Be("Monokai");
        (await File.ReadAllTextAsync(_jsonPath)).Should().NotBe(originalContent);
    }

    [Fact]
    public async Task Revert_AfterPropertyChange_ReturnsValueToFlushedState()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();
        var persistedFontSize = systemUnderTests.Value.CodeFontSize;

        // Act
        systemUnderTests.Value.CodeFontSize = 60;
        systemUnderTests.Revert();

        // Assert
        systemUnderTests.Value.CodeFontSize.Should().Be(persistedFontSize);
        (await ReadFileAsync()).CodeFontSize.Should().Be(persistedFontSize);
    }

    [Fact]
    public async Task Revert_AfterFlush_ReturnsValueToNewBaseline()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();

        // Act
        systemUnderTests.Value.CodeFontSize = 33;
        await systemUnderTests.FlushAsync();
        systemUnderTests.Value.CodeFontSize = 44;
        systemUnderTests.Revert();

        // Assert
        systemUnderTests.Value.CodeFontSize.Should().Be(33);
    }

    [Fact]
    public async Task Revert_WithoutChanges_DoesNotThrow()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();
        var expectedFontSize = systemUnderTests.Value.CodeFontSize;

        // Act
        systemUnderTests.Revert();

        // Assert
        systemUnderTests.Value.CodeFontSize.Should().Be(expectedFontSize);
    }

    [Fact]
    public async Task LoadAsync_WhenFileCorrupted_DeletesAndCreatesNewFile()
    {
        // Arrange
        await File.WriteAllTextAsync(_jsonPath, "{ invalid: json }");
        var corruptedContent = await File.ReadAllTextAsync(_jsonPath);
        var systemUnderTests = CreateSystemUnderTests();

        // Act
        await systemUnderTests.LoadAsync();

        // Assert
        var actual = await ReadFileAsync();
        actual.CodeFontSize.Should().Be(_viewDefaultMock.Value.Settings.CodeFontSize);
        actual.CodeTheme.Should().Be(_viewDefaultMock.Value.Settings.CodeTheme);
        (await File.ReadAllTextAsync(_jsonPath)).Should().NotBe(corruptedContent);
    }

    [Fact]
    public async Task ToDefaultAsync_AfterChanges_CopiesDefaultsAndPersists()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();
        systemUnderTests.Value.CodeFontSize = 99;
        systemUnderTests.Value.CodeTheme = "Custom";

        // Act
        await systemUnderTests.ToDefaultAsync();

        // Assert
        systemUnderTests.Value.CodeFontSize.Should().Be(_viewDefaultMock.Value.Settings.CodeFontSize);
        (await ReadFileAsync()).CodeTheme.Should().Be(_viewDefaultMock.Value.Settings.CodeTheme);
    }

    [Fact]
    public async Task FlashAtBackground_WithoutWaiting_WritesFile()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();

        // Act
        systemUnderTests.Value.CodeTheme = "Dracula";
        systemUnderTests.FlashAtBackground();
        await WaitForFileContentAsync("Dracula");

        // Assert
        (await ReadFileAsync()).CodeTheme.Should().Be("Dracula");
    }

    [Fact]
    public async Task FlashAtBackground_WithWaiting_WritesFileAfterCoalescing()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests(0.2);
        await systemUnderTests.LoadAsync();

        // Act
        systemUnderTests.Value.CodeTheme = "First";
        systemUnderTests.FlashAtBackground(true);
        systemUnderTests.Value.CodeTheme = "Second";
        systemUnderTests.FlashAtBackground(true);

        await WaitForFileContentAsync("Second");

        // Assert
        var actual = await ReadFileAsync();
        actual.CodeTheme.Should().Be("Second");
    }

    [Fact]
    public async Task FlushAsync_CalledInParallel_ProducesValidFile()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();

        // Act
        for (var index = 0; index < 50; index++)
        {
            systemUnderTests.Value.CodeFontSize = 10 + index;
            await Task.WhenAll(
                systemUnderTests.FlushAsync().AsTask(),
                systemUnderTests.FlushAsync().AsTask());
        }

        // Assert
        var actual = await ReadFileAsync();
        actual.CodeFontSize.Should().Be(59);
    }

    [Fact]
    public async Task FlushAsync_WhileRevert_KeepsFileReadable()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();
        var persistedFontSize = systemUnderTests.Value.CodeFontSize;

        // Act
        for (var index = 0; index < 100; index++)
        {
            systemUnderTests.Value.CodeFontSize = 20 + index;

            var flushTask = systemUnderTests.FlushAsync().AsTask();
            systemUnderTests.Revert();
            await flushTask;
        }

        // Assert
        var actual = await ReadFileAsync();
        actual.Should().NotBeNull();
        systemUnderTests.Value.CodeFontSize.Should().BeInRange(persistedFontSize, 20 + 99);
    }

    [Fact]
    public async Task RevertWhileFlush_DoesNotLosePersistedBaseline()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();

        // Act
        systemUnderTests.Value.CodeFontSize = 55;
        var flushTask = systemUnderTests.FlushAsync().AsTask();
        systemUnderTests.Revert();
        await flushTask;

        // Assert
        var actual = await ReadFileAsync();
        actual.Should().NotBeNull();
        systemUnderTests.Value.CodeFontSize.Should().BeOneOf(55, 14);
    }

    [Fact]
    public async Task ToDefaultAsync_WhileRevert_EndsWithDefaultValues()
    {
        // Arrange
        var systemUnderTests = CreateSystemUnderTests();
        await systemUnderTests.LoadAsync();

        // Act
        systemUnderTests.Value.CodeFontSize = 77;
        var defaultTask = systemUnderTests.ToDefaultAsync().AsTask();
        systemUnderTests.Revert();
        await defaultTask;

        // Assert
        systemUnderTests.Value.CodeFontSize.Should().Be(_viewDefaultMock.Value.Settings.CodeFontSize);
        (await ReadFileAsync()).CodeFontSize.Should().Be(_viewDefaultMock.Value.Settings.CodeFontSize);
    }

    private async Task<ViewSettings> ReadFileAsync()
    {
        await using var file = File.OpenRead(_jsonPath);
        return await JsonSerializer.DeserializeAsync(file, ViewSettingsSerializerContext.Default.ViewSettings)
               ?? throw new InvalidOperationException("Settings file is empty");
    }

    private async Task WaitForFileContentAsync(string expectedContent)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var content = await File.ReadAllTextAsync(_jsonPath);
            if (content.Contains(expectedContent))
                return;

            await Task.Delay(50);
        }
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }
}
