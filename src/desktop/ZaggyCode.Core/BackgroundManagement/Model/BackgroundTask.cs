namespace ZaggyCode.Core.BackgroundManagement.Model;

public class BackgroundTask
{
    public required Task Task { get; init; }
    public required string Name { get; init; }
    public string? Description { get; set; } = null;
    public required IProgress<double> Progress { get; init; }
}