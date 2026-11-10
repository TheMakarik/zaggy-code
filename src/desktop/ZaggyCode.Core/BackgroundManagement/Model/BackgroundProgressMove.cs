using ZaggyCode.Core.BackgroundManagement.Enums;

namespace ZaggyCode.Core.BackgroundManagement.Model;

public sealed record BackgroundProgressMove(double? NewPercent, BackgroundTaskKind Kind, string? DescriptionOfNextMove);