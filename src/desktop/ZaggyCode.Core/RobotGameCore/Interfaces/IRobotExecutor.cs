namespace ZaggyCode.Core.RobotGameCore.Interfaces;

public interface IRobotExecutor
{
    RobotGameMapState State { get; }
    event EventHandler<RobotStateUpdate>? StateUpdated;
    void Move(RobotDirection direction);
    bool CheckWall(RobotDirection direction);
    void FillCell();
    bool IsCellFilled();
}
