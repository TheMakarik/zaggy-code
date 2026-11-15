namespace ZaggyCode.Modules.BackgroundManagement;

//#:NO_AI
internal sealed class ProgressSubscription(
    Progress<BackgroundProgressMove> progress,
    EventHandler<BackgroundProgressMove> handler) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;

        progress.ProgressChanged -= handler;
        _disposed = true;
    }
}
