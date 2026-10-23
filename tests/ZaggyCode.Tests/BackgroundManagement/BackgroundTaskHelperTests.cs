using ZaggyCode.Core.BackgroundManagement;
using ZaggyCode.Core.BackgroundManagement.Model;
using ZaggyCode.Core.BackgroundManagment.Dto;
using ZaggyCode.Modules.BackgroundManagement;

namespace ZaggyCode.Tests.BackgroundManagement;

public class BackgroundTaskHelperTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    private readonly Fixture _fixture = new();
    private readonly ILogger<IBackgroundTaskHelper> _logger = A.Dummy<ILogger<IBackgroundTaskHelper>>();

    [Fact]
    public async Task StartBackground_WhenCalled_AddsTaskToCollection()
    {
        // Arrange
        using var release = new ManualResetEventSlim();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var dto = new BackgroundTaskStartDto
        {
            Name = _fixture.Create<string>(),
            Description = _fixture.Create<string>(),
            DelegateToStart = _ =>
            {
                started.SetResult();
                release.Wait();
            }
        };
        var systemUnderTests = new BackgroundTaskHelper(_logger);

        try
        {
            // Act
            systemUnderTests.StartBackground(dto);

            // Assert
            await started.Task.WaitAsync(WaitTimeout);
            systemUnderTests.BackgroundTasks.Should().ContainSingle();

            var entry = systemUnderTests.BackgroundTasks.Single();
            entry.Key.Should().Be(entry.Value.Task.Id);
            entry.Value.Name.Should().Be(dto.Name);
            entry.Value.Description.Should().Be(dto.Description);
        }
        finally
        {
            release.Set();
        }
    }

    [Fact]
    public async Task StartBackground_WhenCalled_PassesProgressToDelegate()
    {
        // Arrange
        using var release = new ManualResetEventSlim();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Progress<double>? receivedProgress = null;
        var dto = new BackgroundTaskStartDto
        {
            Name = _fixture.Create<string>(),
            DelegateToStart = progress =>
            {
                receivedProgress = progress;
                started.SetResult();
                release.Wait();
            }
        };
        var systemUnderTests = new BackgroundTaskHelper(_logger);

        try
        {
            // Act
            systemUnderTests.StartBackground(dto);

            // Assert
            await started.Task.WaitAsync(WaitTimeout);

            var backgroundTask = systemUnderTests.BackgroundTasks.Single().Value;
            receivedProgress.Should().NotBeNull();
            receivedProgress.Should().BeSameAs(backgroundTask.Progress);
        }
        finally
        {
            release.Set();
        }
    }

    [Fact]
    public async Task StartBackground_WhenTaskCompletes_RemovesTaskFromCollection()
    {
        // Arrange
        var dto = new BackgroundTaskStartDto
        {
            Name = _fixture.Create<string>(),
            DelegateToStart = _ => { }
        };
        var systemUnderTests = new BackgroundTaskHelper(_logger);

        // Act
        systemUnderTests.StartBackground(dto);

        // Assert
        await WaitUntilAsync(() => systemUnderTests.BackgroundTasks.Count is 0);
        systemUnderTests.BackgroundTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task StartBackground_WhenDelegateReportsProgress_RaisesProgressChangedWithReportedValue()
    {
        // Arrange
        using var release = new ManualResetEventSlim();
        using var reportAllowed = new ManualResetEventSlim();
        var started = new TaskCompletionSource<Progress<double>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var progressReported = new TaskCompletionSource<double>(TaskCreationOptions.RunContinuationsAsynchronously);
        const double expectedProgress = 42.5;
        var dto = new BackgroundTaskStartDto
        {
            Name = _fixture.Create<string>(),
            DelegateToStart = progress =>
            {
                started.SetResult(progress);
                reportAllowed.Wait();
                ((IProgress<double>)progress).Report(expectedProgress);
                release.Wait();
            }
        };
        var systemUnderTests = new BackgroundTaskHelper(_logger);

        try
        {
            // Act
            systemUnderTests.StartBackground(dto);

            var delegateProgress = await started.Task.WaitAsync(WaitTimeout);
            var backgroundTask = systemUnderTests.BackgroundTasks.Single().Value;
            backgroundTask.Progress.Should().BeOfType<Progress<double>>();

            var progress = (Progress<double>)backgroundTask.Progress;
            progress.Should().BeSameAs(delegateProgress);
            progress.ProgressChanged += (_, value) => progressReported.TrySetResult(value);

            reportAllowed.Set();

            // Assert
            var actualProgress = await progressReported.Task.WaitAsync(WaitTimeout);
            actualProgress.Should().Be(expectedProgress);
        }
        finally
        {
            release.Set();
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        while (!condition())
        {
            if (Stopwatch.GetElapsedTime(startTimestamp) > WaitTimeout)
                throw new TimeoutException("Условие не выполнилось за отведённое время");

            await Task.Delay(10);
        }
    }
}
