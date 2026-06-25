using ZaggyCode.Games.Events;
using ZaggyCode.Games.Models;

namespace ZaggyCode.Games.Interfaces;

public interface IRobotMover
{
    public void Left();
    public void Up();
    public void Down();
    public void Right();
}