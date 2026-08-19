namespace ZaggyCode.Modules.Data;

//#:NO_AI
public sealed class UpdateStorageWaiter(ILogger<UpdateStorageWaiter> logger) : IUpdateStorageWaiter
{
    private long _version;
    private long _lastFlushedVersion;

    public void BeginObserve(INotifyPropertyChanged observable, Func<ValueTask> flushAsync, TimeSpan interval)
    {
        observable.PropertyChanged += (_, args) =>
        {
            logger.LogInformation("Property changed: {propertyName}", args.PropertyName);
            Interlocked.Increment(ref _version);
        };

        Task.Factory.StartNew(async () =>
        {
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync())
            {
                var currentVersion = Interlocked.Read(ref _version);
                if (currentVersion == _lastFlushedVersion)
                    continue;

                await flushAsync();
                Interlocked.Exchange(ref _lastFlushedVersion, currentVersion);
                logger.LogInformation("Updates flushed successfully");
            }
        }, TaskCreationOptions.LongRunning);
    }
}
