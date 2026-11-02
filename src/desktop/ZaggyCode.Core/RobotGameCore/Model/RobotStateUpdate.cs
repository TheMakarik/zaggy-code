using System.Drawing;
using ZaggyCode.Core.RobotGameCore.Enums;

namespace ZaggyCode.Core.RobotGameCore.Model;

//Direction - плавное движение на одну клетку, может существовать вместе с DeadType, тогда смерть произошла при попытке движение 
//и значит что нужно показать неудачную анимацию движения перед смертью
//RobotDeadType - то, как робот умер
//ForcePointMovement - может быть только если первые два свойства null - принудительное движение робота в определнных x,y
public readonly record struct RobotStateUpdate(RobotDirection? Direction, RobotDeadType? DeadType, Point? ForcePointMovement);