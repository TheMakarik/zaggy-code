using ZaggyCode.Core.LanguageRuntime.Model;

namespace ZaggyCode.Core.RobotGameCore.Model;

public record RobotGameResult(
    bool Win,
    bool Interrupted,
    ulong TotalLines, 
    ulong PointsDrew, 
    ulong PointsWalked,
    int CoinsCollected,
    int CoinsTotal,
    RobotDeadType? DeadType,
    CodeError? Error);