namespace ZaggyCode.Core.Languages.Interfaces;

//#:NO_AI
public interface IPythonFunctionNameValidator
{
    PythonFunctionNameValidationResult Validate(string functionName);
}
