namespace ZaggyCode.Core.Languages.Model;

public sealed record ComputedLineOfCode(int LineNumber, RobotStateUpdate Update, string? ErrorMessage = null)
{
    public bool ErrorOccured => ErrorMessage is not null;
};