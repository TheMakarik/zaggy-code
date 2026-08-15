namespace ZaggyCode.Modules.Languages.Options;

public sealed class PythonValidationOptions
{
    public required string[] ForbiddenCharacters { get; set; }
    public required string[] GlobalFunctions { get; set; }
    public required string[] SkipFiles { get; set; }
}
