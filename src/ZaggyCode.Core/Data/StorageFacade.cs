namespace ZaggyCode.Core.Data;

public sealed class StorageFacade(IUserStorage userStorage, IGameCodeStorage gameCodeStorage) : IStorageFacade
{
    public Task LoadAllAsync()
    {
        return userStorage.LoadAsync()
            .ContinueWith((_) => gameCodeStorage.LoadAsync());
    }

    public async ValueTask FlushAllAsync()
    {
        await userStorage.FlushUpdatesAsync();
        await gameCodeStorage.FlushUpdatesAsync();
    }
}