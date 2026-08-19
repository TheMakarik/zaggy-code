namespace ZaggyCode.Core.Languages.Interfaces;

public interface IPythonFunctionNameValidator
{
    PythonFunctionNameValidationResult Validate(string functionName);
}
