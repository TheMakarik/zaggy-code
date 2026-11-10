using ZaggyCode.Core.LanguageRuntime.Dto;
using ZaggyCode.Core.LanguageRuntime.Model;

namespace ZaggyCode.Modules.LanguageRuntime;

public abstract class LanguageRunnerBase(LanguageRunnerDto dto) : ILanguageRunner
{
    private volatile ExecutionSpeed _speed = dto.Speed;

    protected TextReader Input { get; private set; } = dto.Input;
    protected TextWriter Output { get; private set; } = dto.Output;
    protected IRobotController RobotController { get; private set; } = dto.RobotController;

    public ExecutionSpeed Speed
    {
        get => _speed;
        set => _speed = value;
    }

    // Раннер живёт между запусками, поэтому потоки терминала и контроллер
    // нужно переназначать на каждый новый запуск.
    public virtual void Configure(LanguageRunnerDto dto)
    {
        Input = dto.Input;
        Output = dto.Output;
        RobotController = dto.RobotController;
        Speed = dto.Speed;
    }

    public abstract IAsyncEnumerable<ComputedLineOfCode> ExecuteAsync(
        string code,
        CancellationToken cancellationToken = default);

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
