using ZaggyCode.Core.RobotGameCore.Enums;
using ZaggyCode.Core.RobotGameCore.Interfaces;

namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotController : IRobotController
{
    private const string ExecutorMissingMessage = "Исполнитель робота не установлен.";

    public IRobotExecutor? Executor { get; set; }

    private IRobotExecutor RequiredExecutor => Executor
        ?? throw new InvalidOperationException(ExecutorMissingMessage);

    public void MoveUp() => RequiredExecutor.Move(RobotDirection.Up);

    public void MoveRight() => RequiredExecutor.Move(RobotDirection.Right);

    public void MoveDown() => RequiredExecutor.Move(RobotDirection.Down);

    public void MoveLeft() => RequiredExecutor.Move(RobotDirection.Left);

    public void FillCell() => RequiredExecutor.FillCell();

    public bool IsCellFilled() => RequiredExecutor.IsCellFilled();

    public bool IsWallFromUp() => RequiredExecutor.CheckWall(RobotDirection.Up);

    public bool IsWallFromDown() => RequiredExecutor.CheckWall(RobotDirection.Down);

    public bool IsWallFromLeft() => RequiredExecutor.CheckWall(RobotDirection.Left);

    public bool IsWallFromRight() => RequiredExecutor.CheckWall(RobotDirection.Right);
}
