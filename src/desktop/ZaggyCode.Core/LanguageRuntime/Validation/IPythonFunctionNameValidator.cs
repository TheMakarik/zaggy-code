namespace ZaggyCode.Core.LanguageRuntime.Validation;

public interface IPythonFunctionNameValidator
{
    PythonFunctionNameValidationResult Validate(string functionName);
}
