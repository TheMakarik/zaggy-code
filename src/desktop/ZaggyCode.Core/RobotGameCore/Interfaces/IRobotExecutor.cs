using ZaggyCode.Core.RobotGameCore.Enums;
using ZaggyCode.Core.RobotGameCore.Model;

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
