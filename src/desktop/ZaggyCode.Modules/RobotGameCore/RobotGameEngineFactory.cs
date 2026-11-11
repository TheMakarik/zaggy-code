using ZaggyCode.Core.LanguageRuntime.Extensions;
using ZaggyCode.Core.RobotGameCore.Dto;
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
            DelegateToStart = progress =>
            {
                CreateEngine(dto, cancellationToken, progress, completion);
                return Task.CompletedTask;
            }
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
            var map = dto.Map;

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
                backgroundTaskHelper,
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
