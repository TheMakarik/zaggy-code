using ZaggyCode.Data.Interfaces;

namespace ZaggyCode.Data;

public sealed class StorageFacade(IUserStorage userStorage, IGameCodeStorage gameCodeStorage) : IStorageFacade
{
    public async Task LoadAllAsync(IProgress<int> progress)
    {
        await userStorage.LoadAsync();
        progress.Report(50);
        await gameCodeStorage.LoadAsync();
        progress.Report(100);
    }

    public async ValueTask FlushAllAsync()
    {
        await userStorage.FlushUpdatesAsync();
        await gameCodeStorage.FlushUpdatesAsync();
    }
}