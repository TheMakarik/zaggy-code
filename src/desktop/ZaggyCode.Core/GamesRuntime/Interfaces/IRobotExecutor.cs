using ZaggyCode.Core.GamesRuntime.Attributes;

namespace ZaggyCode.Core.GamesRuntime.Interfaces;

//#:NO_AI
public interface IRobotExecutor
{
    public EventHandler<RobotPointUpdatedEventArgs>? RobotPointUpdated { get; set; }
    public EventHandler<RobotDeadEventArgs>? RobotDied { get; set; }
    public EventHandler<DrawPointEventArgs>? DrawPoint { get; set; }
    
    [AllowUseInRobot]
    void MoveUp();
    
    [AllowUseInRobot]
    void MoveRight();
    
    [AllowUseInRobot]
    void MoveDown();
    
    [AllowUseInRobot]
    void MoveLeft();
    
    [AllowUseInRobot]
    void FillCell();
    
    [AllowUseInRobot]
    bool IsCellFilled();
    
    [AllowUseInRobot]
    bool IsWallFromUp();
    
    [AllowUseInRobot]
    bool IsWallFromDown();
    
    [AllowUseInRobot]
    bool IsWallFromLeft();
    
    [AllowUseInRobot]
    bool IsWallFromRight();
}
