namespace ZaggyCode.Modules.Languages;

public sealed class LanguageSleepHelper : ILanguageSleepHelper
{
    public void Sleep(int timeMilliseconds, int chunkMilliseconds, CancellationToken token)
    {
        Debug.Assert(Environment.CurrentManagedThreadId != 1, "Cannot use Thread.Sleep from main application thread because app will be frozen");
        Debug.Assert(timeMilliseconds >= 0, "Time must be positive");
        Debug.Assert(chunkMilliseconds >= 1, "Chunk cannot be negative or zero (exception will be occured)");

        var chunksCount = timeMilliseconds / chunkMilliseconds;
        
        for(var i = 0; i < chunksCount; i++)
        {
            token.ThrowIfCancellationRequested();
            Thread.Sleep(chunkMilliseconds);
        }
    }
}