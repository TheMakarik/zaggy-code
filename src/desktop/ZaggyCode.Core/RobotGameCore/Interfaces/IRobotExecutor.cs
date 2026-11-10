using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Core.RobotGameCore.Interfaces;

public interface IRobotExecutor
{
    RobotGameMapState State { get; }
    event EventHandler<RobotStateUpdate>? StateUpdated;
    void MoveUp();
    void MoveRight();
    void MoveDown();
    void MoveLeft();
    void FillCell();
    bool IsCellFilled();
    bool IsWallFromUp();
    bool IsWallFromDown();
    bool IsWallFromLeft();
    bool IsWallFromRight();
}
