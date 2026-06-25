using ZaggyCode.Games.Models;

namespace ZaggyCode.Games.Interfaces;

public interface IGameEditor : IAsyncDisposable, IDisposable
{
    public Game OpenEditable(string path);
}