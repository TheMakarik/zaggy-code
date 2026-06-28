using ZaggyCode.Data.Interfaces;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Data;

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