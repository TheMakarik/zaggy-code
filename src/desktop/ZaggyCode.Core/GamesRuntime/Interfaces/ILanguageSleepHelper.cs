namespace ZaggyCode.Core.GamesRuntime.Interfaces;

//#:NO_AI
public interface ILanguageSleepHelper
{
    public void Sleep(int timeMilliseconds, int chunkMilliseconds, CancellationToken token);
}