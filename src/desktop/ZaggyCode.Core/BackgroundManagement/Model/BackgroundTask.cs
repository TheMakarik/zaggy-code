using ZaggyCode.Core.BackgroundManagement.Enums;

namespace ZaggyCode.Core.BackgroundManagement.Model;

public class BackgroundTask
{
    public required BackgroundTaskKind Kind { get; init; }
    public required Task Task { get; init; }
    public string? Description { get; set; } = null;
    public required IProgress<BackgroundProgressMove> Progress { get; init; }
}
