namespace ZaggyCode.Modules.Languages.Validation;

public interface IPythonFunctionNameValidator
{
    PythonFunctionNameValidationResult Validate(string functionName);
}
