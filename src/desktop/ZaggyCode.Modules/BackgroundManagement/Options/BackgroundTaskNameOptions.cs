namespace ZaggyCode.Modules.BackgroundManagement.Options;

public sealed class BackgroundTaskNameOptions
{
    public Dictionary<BackgroundTaskKind, string> Names { get; set; } = [];
}
