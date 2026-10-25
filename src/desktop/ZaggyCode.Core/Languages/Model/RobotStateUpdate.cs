using System.Drawing;

namespace ZaggyCode.Core.Languages.Model;

//Direction - плавное движение на одну клетку, может существовать вместе с DeadType, тогда смерть произошла при попытке движение 
//и значит что нужно показать неудачную анимацию движения перед смертью
//RobotDeadType - то, как робот умер
//ForcePointMovement - может быть только если первые два свойства null - принудительное движение робота в определнных x,y
public record RobotStateUpdate(Direction? Direction, RobotDeadType? DeadType, Point? ForcePointMovement);