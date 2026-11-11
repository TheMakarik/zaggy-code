namespace ZaggyCode.Modules.LanguageRuntime;

public abstract class LanguageRunnerBase : ILanguageRunner
{
    private volatile ExecutionSpeed _speed = ExecutionSpeed.X1;

    protected TextReader Input { get; private set; } = TextReader.Null;
    protected TextWriter Output { get; private set; } = TextWriter.Null;

    // Контроллер приходит только через Configure перед запуском, поэтому до него
    // обращаться к свойству некому; null! избавляет от лишних проверок в наследниках.
    protected IRobotController RobotController { get; private set; } = null!;

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
