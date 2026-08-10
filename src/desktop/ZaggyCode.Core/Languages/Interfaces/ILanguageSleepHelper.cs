namespace ZaggyCode.Core.Languages.Interfaces;

public interface ILanguageSleepHelper
{
    public void Sleep(int timeMilliseconds, int chunkMilliseconds, CancellationToken token);

}