namespace ZaggyCode.Core.RobotGameCore.Model;

// Actions - действия робота за одну строку кода: в строке может быть несколько вызовов методов робота,
// поэтому это коллекция, а не одно действие; null, если строка не трогала робота.
// Смерть приходит внутри самого действия (RobotAction.DeadType), чтобы движок знал, на каком именно
// действии робот умер, и успел проиграть неудачную анимацию движения перед смертью.
public readonly record struct RobotStateUpdate(IReadOnlyCollection<RobotAction>? Actions)
{
    public static RobotStateUpdate From(RobotAction action) => new([action]);
}
