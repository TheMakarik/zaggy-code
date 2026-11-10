using ZaggyCode.Core.BackgroundManagement;
using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Enums;
using ZaggyCode.Core.BackgroundManagement.Model;
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
            Kind = _fixture.Create<BackgroundTaskKind>(),
            HasNoProgress = true,
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
            entry.Key.Should().Be(dto.Kind);
            entry.Value.Kind.Should().Be(dto.Kind);
            entry.Value.HasNoProgress.Should().BeTrue();
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
        Progress<BackgroundProgressMove>? receivedProgress = null;
        var dto = new BackgroundTaskStartDto
        {
            Kind = _fixture.Create<BackgroundTaskKind>(),
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
            Kind = _fixture.Create<BackgroundTaskKind>(),
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
    public async Task StartBackground_WhenDelegateReportsProgress_RaisesProgressChangedWithReportedMove()
    {
        // Arrange
        using var release = new ManualResetEventSlim();
        using var reportAllowed = new ManualResetEventSlim();
        var started = new TaskCompletionSource<Progress<BackgroundProgressMove>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var progressReported = new TaskCompletionSource<BackgroundProgressMove>(TaskCreationOptions.RunContinuationsAsynchronously);
        var kind = _fixture.Create<BackgroundTaskKind>();
        var expectedMove = new BackgroundProgressMove(
            42.5,
            kind,
            _fixture.Create<string>());
        var dto = new BackgroundTaskStartDto
        {
            Kind = kind,
            DelegateToStart = progress =>
            {
                started.SetResult(progress);
                reportAllowed.Wait();
                ((IProgress<BackgroundProgressMove>)progress).Report(expectedMove);
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
            backgroundTask.Progress.Should().BeOfType<Progress<BackgroundProgressMove>>();

            var progress = (Progress<BackgroundProgressMove>)backgroundTask.Progress;
            progress.Should().BeSameAs(delegateProgress);
            progress.ProgressChanged += (_, move) => progressReported.TrySetResult(move);

            reportAllowed.Set();

            // Assert
            var actualMove = await progressReported.Task.WaitAsync(WaitTimeout);
            actualMove.Should().Be(expectedMove);
            backgroundTask.Description.Should().Be(expectedMove.DescriptionOfNextMove);
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
