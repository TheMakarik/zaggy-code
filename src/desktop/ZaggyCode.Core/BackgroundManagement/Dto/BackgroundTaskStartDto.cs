using ZaggyCode.Core.BackgroundManagement.Enums;
using ZaggyCode.Core.BackgroundManagement.Model;

namespace ZaggyCode.Core.BackgroundManagement.Dto;

public sealed class BackgroundTaskStartDto
{
    public required Action<Progress<BackgroundProgressMove>> DelegateToStart { get; init; }
    public BackgroundTaskKind Kind { get; init; }
}