namespace ZaggyCode.Core.Data.Interfaces;

//#:NO_AI
public interface IGameCodeStorage : IStorage
{
    public void AddGameCode(string gamePath, string code, Language language);
    public ValueTask<string?> GetGameCodeAsync(string gamePath, Language language);
}
