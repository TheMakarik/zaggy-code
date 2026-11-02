using ZaggyCode.Core.LanguageRuntime.Enums;

namespace ZaggyCode.Core.LanguageRuntime.Model;

public readonly struct CodeError(CodeErrorType codeErrorType, string? errorMessage = null)
{
    public CodeErrorType CodeErrorType { get; } = codeErrorType;
    public string? ErrorMessage { get; } = errorMessage;
}