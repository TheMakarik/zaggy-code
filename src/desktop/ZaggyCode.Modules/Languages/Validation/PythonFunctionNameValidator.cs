namespace ZaggyCode.Modules.Languages.Validation;

public class PythonFunctionNameValidator(IOptions<PythonValidationOptions> options, IOptions<PythonScriptsOptions> scriptsOptions, ILogger<PythonFunctionNameValidator> logger) : IPythonFunctionNameValidator
{
    public PythonFunctionNameValidationResult Validate(string functionName)
    {
        return PythonFunctionNameValidationResult.Empty;
    }
}
