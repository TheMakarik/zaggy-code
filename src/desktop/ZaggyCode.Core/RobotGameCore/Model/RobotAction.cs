namespace ZaggyCode.Core.RobotGameCore.Model;

public record RobotAction(RobotDirection Direction, bool CoinCollected, bool PointDrew, RobotDeadType? DeadType);