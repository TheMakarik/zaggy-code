namespace ZaggyCode.Core.LanguageRuntime.Validation;

public enum PythonFunctionNameValidationResult
{
    Success,
    Empty,
    ContainsSpaces,
    ContainsForbiddenCharacters,
    StartsWithDigit,
    StartsWithUnderscore,
    IsReservedGlobalFunction,
    IsStandardLibraryModule,
    IsStandardLibraryFunction
}
