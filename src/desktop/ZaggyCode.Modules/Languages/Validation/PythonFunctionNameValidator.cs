namespace ZaggyCode.Modules.Languages.Validation;

public sealed class PythonFunctionNameValidator(
    IOptions<PythonValidationOptions> options,
    IOptions<PythonScriptsOptions> scriptsOptions,
    ILogger<PythonFunctionNameValidator> logger) : IPythonFunctionNameValidator
{
    private readonly HashSet<string> _globalFunctions = new(options.Value.GlobalFunctions, StringComparer.Ordinal);
    private readonly HashSet<string> _moduleNames = new(StringComparer.Ordinal);
    private readonly HashSet<string> _functionNames = new(StringComparer.Ordinal);

    public PythonFunctionNameValidationResult Validate(string functionName)
    {
        if (string.IsNullOrWhiteSpace(functionName))
            return PythonFunctionNameValidationResult.Empty;

        if (functionName.Contains(' '))
            return PythonFunctionNameValidationResult.ContainsSpaces;

        if (options.Value.ForbiddenCharacters.Any(functionName.Contains))
            return PythonFunctionNameValidationResult.ContainsForbiddenCharacters;

        if (char.IsDigit(functionName[0]))
            return PythonFunctionNameValidationResult.StartsWithDigit;

        if (functionName[0] == '_')
            return PythonFunctionNameValidationResult.StartsWithUnderscore;

        LoadStandardLibraryNames();

        if (_globalFunctions.Contains(functionName))
            return PythonFunctionNameValidationResult.IsReservedGlobalFunction;

        if (_moduleNames.Contains(functionName))
            return PythonFunctionNameValidationResult.IsStandardLibraryModule;

        if (_functionNames.Contains(functionName))
            return PythonFunctionNameValidationResult.IsStandardLibraryFunction;

        return PythonFunctionNameValidationResult.Success;
    }

    private void LoadStandardLibraryNames()
    {
        if (_moduleNames.Count > 0)
            return;

        var stdlibPath = scriptsOptions.Value.StandardLibraryPath;
        if (!Directory.Exists(stdlibPath))
        {
            logger.LogWarning("Standard library path does not exist: {path}", stdlibPath);
            return;
        }

        var skipFiles = new HashSet<string>(options.Value.SkipFiles, StringComparer.OrdinalIgnoreCase);

        foreach (var filePath in Directory.EnumerateFiles(stdlibPath, "*.py", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(filePath);
            if (skipFiles.Contains(fileName))
                continue;

            var moduleName = Path.GetFileNameWithoutExtension(filePath);
            _moduleNames.Add(moduleName);

            foreach (var line in File.ReadLines(filePath))
            {
                var trimmed = line.TrimStart();
                if (!trimmed.StartsWith("def ", StringComparison.Ordinal))
                    continue;

                var afterDef = trimmed[4..].TrimStart();
                var parenIndex = afterDef.IndexOf('(');
                if (parenIndex <= 0)
                    continue;

                var name = afterDef[..parenIndex].Trim();
                if (!string.IsNullOrEmpty(name))
                    _functionNames.Add(name);
            }
        }
    }
}
