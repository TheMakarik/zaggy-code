using ZaggyCode.Games.Models;
using ZaggyCode.Languages.Enums;

namespace ZaggyCode.Data.Interfaces;

public interface IGameCodeStorage : IStorage
{
    public void AddGameCode(string gamePath, string code, Language language);
    public ValueTask<string?> GetGameCodeAsync(string gamePath, Language language);
}