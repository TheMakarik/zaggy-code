using ZaggyCode.Core.GamesRuntime.Model.GameObjects;

namespace ZaggyCode.Core.GamesRuntime.Interfaces;

//#:NO_AI
public interface IRobotyGameEditor : IAsyncDisposable, IDisposable
{
    public Game OpenEditable(string path);
}
