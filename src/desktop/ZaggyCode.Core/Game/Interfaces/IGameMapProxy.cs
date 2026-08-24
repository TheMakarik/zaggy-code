namespace ZaggyCode.Core.Game.Interfaces;

//Должен быть имплементировал для общения карты и IGameEngine, его реализация работает с Map 
//На фронтенде, а абстракция работает с IGameEngine
public interface IGameMapProxy
{
    //Получает Point, при изменении которого срабатывает INPC или INCC и изменяет карту.
    //Должен быть защищен от изменения X и Y (исключением)
    public Point? GetObservablePoint();
    
    //Получает Map который также поднимает INPC
    //Должен быть защищен от изменения Points (исключением)
    public Map? GetObservableMap();

    //Получает наблюдаемую Game
    //Долджа быть защищена от изменения Maps (исключением)
    public Models.Game? GetObservableGame();

    //Убить робота, вызывается IGameEngine, должен показывать анимацию смерти
    //Анимация может сопровождаться неудачной попыткой движения, если Direction не null
    public void RobotDead(Direction? whenMovedTo);

    //Двигает робота на определенные кординаты
    public void MoveRobot(int newX, int newY);
}