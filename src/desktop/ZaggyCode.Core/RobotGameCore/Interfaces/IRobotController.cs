namespace ZaggyCode.Core.RobotGameCore.Interfaces;

public interface IRobotController
{
    IRobotExecutor Executor { get; set; }
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
