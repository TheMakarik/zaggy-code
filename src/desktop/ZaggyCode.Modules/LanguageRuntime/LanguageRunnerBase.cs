using ZaggyCode.Core.LanguageRuntime.Model;
using ZaggyCode.Core.RobotGameCore.Interfaces;

namespace ZaggyCode.Modules.LanguageRuntime;

public abstract class LanguageRunnerBase(LanguageRunnerDto dto, IOptions<SpeedMillisecondsOptions> speedOptions) : ILanguageRunner
{
    private volatile ExecutionSpeed _speed = dto.Speed;

    protected TextReader Input { get; } = dto.Input;
    protected TextWriter Output { get; } = dto.Output;
    protected IRobotController RobotController { get; } = dto.RobotController;

    public ExecutionSpeed Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public abstract IAsyncEnumerable<ComputedLineOfCode> ExecuteAsync(
        string code,
        CancellationToken cancellationToken = default);

    protected Task PaceAsync(CancellationToken cancellationToken)
        => Task.Delay(Speed.GetActual(speedOptions.Value), cancellationToken);

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
