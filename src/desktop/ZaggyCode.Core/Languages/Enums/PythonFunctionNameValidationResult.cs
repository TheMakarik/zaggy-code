namespace ZaggyCode.Core.Languages.Enums;

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
