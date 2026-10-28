namespace ZaggyCode.Core.GamesRuntime.Enums;

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
