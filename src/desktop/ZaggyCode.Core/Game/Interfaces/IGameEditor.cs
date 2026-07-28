namespace ZaggyCode.Core.Game.Interfaces;

public interface IGameEditor : IAsyncDisposable, IDisposable
{
    public Game.Models.Game OpenEditable(string path);
}
