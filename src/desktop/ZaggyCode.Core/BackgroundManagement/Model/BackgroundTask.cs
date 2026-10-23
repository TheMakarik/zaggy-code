namespace ZaggyCode.Core.BackgroundManagement.Model;

public sealed record BackgroundTask
{
    public required Task Task { get; init; }
    public required string Name { get; init; }
    public string? Description { get; set; } = null;
    public required IProgress<double> Progress { get; init; }
}