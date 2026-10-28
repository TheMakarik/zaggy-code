namespace ZaggyCode.Core.GamesRuntime.Model;

public readonly ref struct CodeError(CodeErrorType codeErrorType, ReadOnlySpan<char> errorMessage = default)
{
    public CodeErrorType  CodeErrorType { get; } = codeErrorType;
    public ReadOnlySpan<char> ErrorMessage { get; } = errorMessage;
}