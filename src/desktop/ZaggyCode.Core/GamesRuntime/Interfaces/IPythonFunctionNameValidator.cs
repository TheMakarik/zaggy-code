namespace ZaggyCode.Core.GamesRuntime.Interfaces;

//#:NO_AI
public interface IPythonFunctionNameValidator
{
    PythonFunctionNameValidationResult Validate(string functionName);
}
