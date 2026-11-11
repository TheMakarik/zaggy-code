namespace ZaggyCode.Core.BackgroundManagement.Dto;

public sealed class BackgroundTaskStartDto
{
    public required Func<Progress<BackgroundProgressMove>, Task> DelegateToStart { get; init; }
    public BackgroundTaskKind Kind { get; init; }
    public bool HasNoProgress { get; init; }
}
