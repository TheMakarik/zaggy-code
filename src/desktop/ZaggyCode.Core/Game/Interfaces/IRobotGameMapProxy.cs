namespace ZaggyCode.Core.Game.Interfaces;

//Должен быть имплементировал для общения карты и IGameEngine, его реализация работает с Map 
//На фронтенде, а абстракция работает с IGameEngine
public interface IRobotGameMapProxy
{
    //Получает Point, при изменении которого срабатывает INPC или INCC и изменяет карту.
    //Должен быть защищен от изменения X и Y (исключением)
    public RobotGamePoint? GetObservablePoint();
    
    //Получает Map который также поднимает INPC
    //Должен быть защищен от изменения Points (исключением)
    public Map? GetObservableMap();

    //Получает наблюдаемую Game
    //Долджа быть защищена от изменения Maps (исключением)
    public Models.Game? GetObservableGame();

    //Убить робота, вызывается IGameEngine, должен показывать анимацию смерти
    //Анимация может сопровождаться неудачной попыткой движения, если Direction не null
    public void RobotDead(RobotDeadType deadType, Direction? whenMovedTo = null);

    //Двигает робота на определенные кординаты
    //Нет смысла по новой определять куда движеться робот поэтому можно передать Direction 
    //Ведь он и так уже будет определен
    public void MoveRobot(int newX, int newY, Direction direction);

    
    //Закрашивает точку
    public void FillPoint(int x, int y);
}