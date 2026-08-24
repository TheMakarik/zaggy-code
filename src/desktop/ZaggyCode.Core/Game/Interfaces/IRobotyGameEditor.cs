namespace ZaggyCode.Core.Game.Interfaces;

//#:NO_AI
public interface IRobotyGameEditor : IAsyncDisposable, IDisposable
{
    public Game.Models.Game OpenEditable(string path);
}
