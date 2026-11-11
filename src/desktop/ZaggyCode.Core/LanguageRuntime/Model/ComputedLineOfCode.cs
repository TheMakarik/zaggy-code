namespace ZaggyCode.Core.LanguageRuntime.Model;

public readonly struct ComputedLineOfCode(int lineNumber, RobotStateUpdate update, CodeError codeError = default)
{
    public int LineNumber { get; } = lineNumber;
    public RobotStateUpdate Update { get; } = update;
    public CodeError CodeError { get; } = codeError;
}