namespace ZaggyCode.Core.RobotGameCore.Model;

// Одно действие робота. Position — состояние робота после действия,
// Direction — попытка движения (null для закраски), DeadType — причина смерти.
public readonly record struct RobotAction(
    RobotDirection? Direction,
    Point Position,
    Point? CollectedCoin,
    Point? DrawnPoint,
    RobotDeadType? DeadType);
