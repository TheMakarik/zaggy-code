using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Enums;
using ZaggyCode.Core.LanguageRuntime.Extensions;
using ZaggyCode.Core.Proxies.Interfaces;
using ZaggyCode.Core.RobotGameCore.Dto;
using ZaggyCode.Core.RobotGameCore.Factories;
using ZaggyCode.Modules.LanguageRuntime;
using ZaggyCode.Modules.LanguageRuntime.Options;

namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotGameEngineFactory(
    IRobotExecutorFactory executorFactory,
    ILanguageRunnerFactory languageRunnerFactory,
    IRobotController robotController,
    IRobotGameMapProxy mapProxy,
    IRobotGameEditorProxy editorProxy,
    IBackgroundTaskHelper backgroundTaskHelper,
    IOptions<SpeedMillisecondsOptions> speedOptions,
    ILogger<RobotGameEngine> logger) : IRobotGameEngineFactory
{
    private const BackgroundTaskKind LoadingTaskKind = BackgroundTaskKind.GameEngineLoading;

    public Task<IRobotGameEngine> CreateAsync(RobotGameEngineInitDto dto, CancellationToken cancellationToken = default)
    {
        var completion = new TaskCompletionSource<IRobotGameEngine>(TaskCreationOptions.RunContinuationsAsynchronously);

        backgroundTaskHelper.StartBackground(new BackgroundTaskStartDto
        {
            Kind = LoadingTaskKind,
            DelegateToStart = progress => CreateEngine(dto, cancellationToken, progress, completion)
        });

        return completion.Task;
    }

    private void CreateEngine(
        RobotGameEngineInitDto dto,
        CancellationToken cancellationToken,
        Progress<BackgroundProgressMove> progress,
        TaskCompletionSource<IRobotGameEngine> completion)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            IProgress<BackgroundProgressMove> reporter = progress;

            reporter.Report(new BackgroundProgressMove(0, LoadingTaskKind, "Выбор карты"));
            var map = dto.Game.Maps.FirstOrDefault()
                ?? throw new InvalidOperationException("В игре нет ни одной карты.");

            reporter.Report(new BackgroundProgressMove(25, LoadingTaskKind, "Создание исполнителя робота"));
            var executor = executorFactory.Create(map);

            reporter.Report(new BackgroundProgressMove(50, LoadingTaskKind, "Настройка контроллера робота"));
            robotController.Executor = executor;

            reporter.Report(new BackgroundProgressMove(75, LoadingTaskKind, "Создание раннера языка"));
            var languageRunner = languageRunnerFactory.Create(dto.ToLanguageRunnerDto(robotController));

            reporter.Report(new BackgroundProgressMove(90, LoadingTaskKind, "Создание игрового движка"));
            var state = new RobotGameState
            {
                Speed = dto.Speed,
                Language = dto.Language,
                Map = map
            };

            var engine = new RobotGameEngine(
                state,
                executor,
                languageRunner,
                mapProxy,
                editorProxy,
                speedOptions,
                logger);

            reporter.Report(new BackgroundProgressMove(100, LoadingTaskKind, "Игровой движок готов"));
            completion.TrySetResult(engine);
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
}
