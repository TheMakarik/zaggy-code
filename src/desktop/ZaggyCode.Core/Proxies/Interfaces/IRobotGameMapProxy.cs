using ZaggyCode.Core.RobotGameCore.Enums;

namespace ZaggyCode.Core.Proxies.Interfaces;

// Обновляет отдельную точку карты: положение робота, собранную монету и закраску.
// Логическое состояние живёт в исполнителе, прокси только сообщает UI, что нужно отрисовать.
public interface IRobotGameMapProxy
{
    void Reset();
    void SetRobotPosition(int x, int y, RobotDirection direction);
    void CollectCoin(int x, int y);
    void MarkDrawn(int x, int y);
    void KillRobot(int x, int y, RobotDirection? attemptedDirection, RobotDeadType deadType);
}
