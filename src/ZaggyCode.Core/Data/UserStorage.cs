namespace ZaggyCode.Core.Data;

public sealed class UserStorage(ILogger<UserStorage> logger, IOptions<StorageOptions> storageOptions, IOptions<DefaultUser> defaultUser, ISpecialFolderProvider folderProvider) : IUserStorage
{
    private readonly Lock _locker = new Lock();
    private  volatile bool _requireUpdate = false;
    public UserData Current { get => field ?? throw new InvalidOperationException("Cannot load null data."); private set; }
    
    public void BeginObserve()
    {
        logger.LogInformation("Observe user data {path}", storageOptions.Value.DataFilePath);
        Current.PropertyChanged += (_, args) =>
        {
            logger.LogInformation("User property changed: {path}", args.PropertyName);
            using Lock.Scope scope = _locker.EnterScope();
            _requireUpdate = true;
        };
        Task.Factory.StartNew(async () =>
        {
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(storageOptions.Value.WaitUserDataUpdateSeconds));
            while (await timer.WaitForNextTickAsync())
                await TryExpandUpdatesAsync();
        }, TaskCreationOptions.LongRunning);
    }
    

    public ValueTask FlushUpdatesAsync()
    {
        return TryExpandUpdatesAsync();
    }

    public async Task LoadAsync()
    {
        storageOptions.Value.DataFilePath = folderProvider.GetFolder(Environment.SpecialFolder.ApplicationData, storageOptions.Value.DataFilePath);
        logger.LogInformation("Begin loading user data from path {path}", storageOptions.Value.DataFilePath);
        if (!File.Exists(storageOptions.Value.DataFilePath))
        {
            await CreateUserConfigFileAsync(storageOptions.Value.DataFilePath);
            BeginObserve();
            return;
        }

        try
        {
            await using FileStream file = File.Open(storageOptions.Value.DataFilePath, FileMode.Open);
            Current = await JsonSerializer.DeserializeAsync<UserData>(file, UserDataSerializerContext.Default.Options) ?? throw new InvalidOperationException("User data file corrupted");
        }
        catch (Exception e)
        {
           logger.LogError(e, "Error loading user data from path {path}", storageOptions.Value.DataFilePath);
           File.Delete(storageOptions.Value.DataFilePath);
           await CreateUserConfigFileAsync(storageOptions.Value.DataFilePath);
        }
        BeginObserve();
    }

    private async Task CreateUserConfigFileAsync(string valueDataFilePath)
    {
        var directory = Path.GetDirectoryName(valueDataFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }
        
        logger.LogInformation("User data was not found. Creating user config file {path}", valueDataFilePath);
        await using FileStream file = File.Create(valueDataFilePath);
        Current = defaultUser.Value.User;
        await JsonSerializer.SerializeAsync(file, defaultUser.Value.User, UserDataSerializerContext.Default.Options);
        using StreamReader streamReader = new StreamReader(file);    
        logger.LogDebug("User config file created successfully: {text}", await streamReader.ReadToEndAsync()) ;
        
    }
    
    private async ValueTask TryExpandUpdatesAsync()
    {
        if (!_requireUpdate)
            return;
        
        await using(FileStream file = File.Open(storageOptions.Value.DataFilePath, FileMode.Truncate))
            await JsonSerializer.SerializeAsync(file, Current, UserDataSerializerContext.Default.Options);
        _requireUpdate = false;
        logger.LogInformation("User config file expanded successfully. Content: {content}", await File.ReadAllTextAsync(storageOptions.Value.DataFilePath));
    }
}