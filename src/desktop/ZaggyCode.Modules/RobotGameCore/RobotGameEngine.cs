using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Enums;
using ZaggyCode.Core.LanguageRuntime.Model;
using ZaggyCode.Core.Proxies.Interfaces;
using ZaggyCode.Modules.LanguageRuntime;

namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotGameEngine(
    RobotGameState state,
    IRobotExecutor executor,
    ILanguageRunner languageRunner,
    IRobotGameMapProxy mapProxy,
    IRobotGameEditorProxy editorProxy,
    IBackgroundTaskHelper backgroundTaskHelper,
    IOptions<SpeedMillisecondsOptions> speedOptions,
    ILogger<RobotGameEngine> logger) : IRobotGameEngine
{
    public RobotGameState State { get; } = state;

    public Task<RobotGameResult> ExecuteAsync(
        string code,
        CancellationToken cancellationToken = default,
        ExecutionType executionType = ExecutionType.ExecuteWithProxies)
    {
        var completion = new TaskCompletionSource<RobotGameResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        backgroundTaskHelper.StartBackground(new BackgroundTaskStartDto
        {
            Kind = BackgroundTaskKind.LanguageExecution,
            HasNoProgress = true,
            DelegateToStart = _ =>
            {
                try
                {
                    var result = ExecuteCoreAsync(code, cancellationToken, executionType)
                        .GetAwaiter()
                        .GetResult();

                    completion.TrySetResult(result);
                }
                catch (OperationCanceledException)
                {
                    completion.TrySetCanceled(cancellationToken);
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            }
        });

        return completion.Task;
    }

    private async Task<RobotGameResult> ExecuteCoreAsync(
        string code,
        CancellationToken cancellationToken,
        ExecutionType executionType)
    {
        var useProxies = executionType == ExecutionType.ExecuteWithProxies;

        State.IsRunning = true;
        languageRunner.Speed = State.Speed;

        if (useProxies)
            mapProxy.Reset();

        var pendingActions = new List<RobotAction>();
        var pendingLock = new Lock();

        void OnStateUpdated(object? sender, RobotStateUpdate update)
        {
            if (update.Actions is null)
                return;

            using var scope = pendingLock.EnterScope();
            pendingActions.AddRange(update.Actions);
        }

        executor.StateUpdated += OnStateUpdated;

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var totalLines = 0UL;
        var pointsDrew = 0UL;
        var pointsWalked = 0UL;
        var coinsCollected = 0;
        var interrupted = false;
        RobotDeadType? deadType = null;
        CodeError? error = null;

        try
        {
            await foreach (var line in languageRunner.ExecuteAsync(code, linkedCancellation.Token))
            {
                if (line.CodeError.CodeErrorType != CodeErrorType.None)
                    error ??= line.CodeError;

                if (line.LineNumber > 0)
                {
                    totalLines++;

                    if (useProxies)
                        editorProxy.SetCurrentExecutionLine(line.LineNumber);
                }

                var actions = TakePendingActions(pendingActions, pendingLock);

                if (useProxies)
                    ApplyActions(actions);

                var deadInLine = Accumulate(actions, ref pointsDrew, ref pointsWalked, ref coinsCollected, ref deadType);

                if (deadInLine)
                {
                    await linkedCancellation.CancelAsync();
                    break;
                }

                await DelayAsync(linkedCancellation.Token);
            }

            var tailActions = TakePendingActions(pendingActions, pendingLock);

            if (useProxies)
                ApplyActions(tailActions);

            Accumulate(tailActions, ref pointsDrew, ref pointsWalked, ref coinsCollected, ref deadType);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            interrupted = true;
        }
        finally
        {
            executor.StateUpdated -= OnStateUpdated;

            if (useProxies)
                editorProxy.SetCurrentExecutionLine(0);

            State.IsRunning = false;
        }

        var result = CreateResult(deadType, error, totalLines, pointsDrew, pointsWalked, coinsCollected, interrupted);

        logger.LogInformation(
            "Игра завершена: победа = {Win}, строк = {Lines}, закрашено = {Drew}, пройдено = {Walked}",
            result.Win,
            result.TotalLines,
            result.PointsDrew,
            result.PointsWalked);

        return result;
    }

    private RobotGameResult CreateResult(
        RobotDeadType? deadType,
        CodeError? error,
        ulong totalLines,
        ulong pointsDrew,
        ulong pointsWalked,
        int coinsCollected,
        bool interrupted)
    {
        var map = State.Map;
        var mapState = executor.State;

        var coinsTotal = map?.Points.Count(point => point.HasCoin) ?? 0;
        var allCoinsCollected = map?.Points
            .Where(point => point.HasCoin)
            .All(point => mapState.IsCoinCollected(new Point(point.X, point.Y))) ?? true;

        var allRequiredDrawn = map?.Points
            .Where(point => point.RequireDraw)
            .All(point => mapState.IsDrawn(new Point(point.X, point.Y))) ?? true;

        var goal = map?.Points.FirstOrDefault(point => point.IsGoal);
        var onGoal = goal is null || (goal.X == mapState.Position.X && goal.Y == mapState.Position.Y);

        var win = deadType is null && error is null && !interrupted
            && allCoinsCollected && allRequiredDrawn && onGoal;

        return new RobotGameResult(
            win,
            interrupted,
            totalLines,
            pointsDrew,
            pointsWalked,
            coinsCollected,
            coinsTotal,
            deadType,
            error);
    }

    private static List<RobotAction> TakePendingActions(List<RobotAction> pendingActions, Lock pendingLock)
    {
        using var scope = pendingLock.EnterScope();

        if (pendingActions.Count == 0)
            return [];

        List<RobotAction> actions = [.. pendingActions];
        pendingActions.Clear();
        return actions;
    }

    private void ApplyActions(IReadOnlyCollection<RobotAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.DeadType is { } deadType)
            {
                mapProxy.KillRobot(action.Position.X, action.Position.Y, action.Direction, deadType);
                return;
            }

            if (action.Direction is { } direction)
                mapProxy.SetRobotPosition(action.Position.X, action.Position.Y, direction);

            if (action.CollectedCoin is { } coin)
                mapProxy.CollectCoin(coin.X, coin.Y);

            if (action.DrawnPoint is { } drawnPoint)
                mapProxy.MarkDrawn(drawnPoint.X, drawnPoint.Y);
        }
    }

    private static bool Accumulate(
        IReadOnlyCollection<RobotAction> actions,
        ref ulong pointsDrew,
        ref ulong pointsWalked,
        ref int coinsCollected,
        ref RobotDeadType? deadType)
    {
        var dead = false;

        foreach (var action in actions)
        {
            if (action.Direction is not null && action.DeadType is null)
                pointsWalked++;

            if (action.DrawnPoint is not null)
                pointsDrew++;

            if (action.CollectedCoin is not null)
                coinsCollected++;

            if (action.DeadType is { } type)
            {
                deadType = type;
                dead = true;
                // После смерти робота остальные действия из той же пачки уже не выполнялись.
                break;
            }
        }

        return dead;
    }

    private async Task DelayAsync(CancellationToken cancellationToken)
    {
        var milliseconds = State.Speed.GetActual(speedOptions.Value);
        var chunk = Math.Max(1, speedOptions.Value.SleepChunk);

        while (milliseconds > 0)
        {
            var currentChunk = Math.Min(chunk, milliseconds);
            await Task.Delay(currentChunk, cancellationToken);
            milliseconds -= currentChunk;
        }
    }
}
