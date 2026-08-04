namespace ZaggyCode.Core.Game.Interfaces;

public interface IRobotExecutor
{
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
