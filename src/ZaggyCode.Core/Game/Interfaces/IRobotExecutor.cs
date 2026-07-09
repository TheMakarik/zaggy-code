namespace ZaggyCode.Core.Game.Interfaces;

public interface IRobotExecutor
{
    void MoveUp();
    void MoveRight();
    void MoveDown();
    void MoveLeft();
    bool CanMoveUp();
    bool CanMoveRight();
    bool CanMoveDown();
    bool CanMoveLeft();
    void Draw();
    bool IsWallFromUp();
    bool IsWallFromDown();
    bool IsWallFromLeft();
    bool IsWallFromRight();
}