namespace ZaggyCode.Core.BackgroundManagement.Dto;

public sealed class BackgroundTaskStartDto
{
    public required Action<Progress<double>> DelegateToStart { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; } = null;
}