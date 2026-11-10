using ZaggyCode.Core.LanguageRuntime.Model;

namespace ZaggyCode.Modules.LanguageRuntime;

public abstract class LanguageRunnerBase(LanguageRunnerDto dto) : ILanguageRunner
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

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
