using ZaggyCode.Core.GamesRuntime.Model.GameObjects;

namespace ZaggyCode.Core.GamesRuntime.Interfaces;

public interface IGameValidator
{
    public bool IsValid(Game game);
}