namespace ZaggyCode.Core.Game.Interfaces;

public interface IGameEditor : IAsyncDisposable, IDisposable
{
    public Models.Game OpenEditable(string path);
}