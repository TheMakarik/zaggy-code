namespace ZaggyCode.Core.GamesRuntime.Interfaces;

//#:NO_AI
public interface IRobotyGameEditor : IAsyncDisposable, IDisposable
{
    public Models.Game OpenEditable(string path);
}
