using ZaggyCode.Core.RobotGameCore.Enums;
using ZaggyCode.Core.RobotGameCore.Interfaces;

namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotController(IRobotExecutor executor) : IRobotController
{
    public IRobotExecutor Executor { get; set; } = executor;

    public void MoveUp() => Executor.Move(RobotDirection.Up);

    public void MoveRight() => Executor.Move(RobotDirection.Right);

    public void MoveDown() => Executor.Move(RobotDirection.Down);

    public void MoveLeft() => Executor.Move(RobotDirection.Left);

    public void FillCell() => Executor.FillCell();

    public bool IsCellFilled() => Executor.IsCellFilled();

    public bool IsWallFromUp() => Executor.CheckWall(RobotDirection.Up);

    public bool IsWallFromDown() => Executor.CheckWall(RobotDirection.Down);

    public bool IsWallFromLeft() => Executor.CheckWall(RobotDirection.Left);

    public bool IsWallFromRight() => Executor.CheckWall(RobotDirection.Right);
}
