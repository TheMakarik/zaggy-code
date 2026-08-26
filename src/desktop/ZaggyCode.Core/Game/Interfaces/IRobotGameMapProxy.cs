namespace ZaggyCode.Core.Game.Interfaces;

//Должен быть имплементировал для общения карты и IGameEngine, его реализация работает с Map 
//На фронтенде, а абстракция работает с IGameEngine
public interface IRobotGameMapProxy
{
    // Убить робота, вызывается IGameEngine, должен показывать анимацию смерти
    // Анимация может сопровождаться неудачной попыткой движения, если Direction не null
    public void RobotDead(RobotDeadType deadType, Direction? whenMovedTo = null);

    // Двигает робота на определенные кординаты
    public void MoveRobot(int newX, int newY);
    public void MoveRobot(Direction direction);

    // Закрашивает точку
    public void FillPoint(int x, int y);
}