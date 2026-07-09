namespace ZaggyCode.Core.Data;

public sealed class GameCodeStorage(
    IUserStorage userStorage,
    ILogger<GameCodeStorage> logger, 
    IOptions<StorageOptions> options) : IGameCodeStorage
{
    private ConcurrentDictionary<string, string> _codes = new();
    private ConcurrentDictionary<string, Task> _creatingFilesTasks = new();
    
    public async ValueTask FlushUpdatesAsync()
    {
        if (_creatingFilesTasks.IsEmpty)
            return;
        
        await Task.WhenAll(_creatingFilesTasks.Values);
    }

    public Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(userStorage.Current.LastGamePath))
            return Task.CompletedTask;
        
        return LoadLastGameCodeAsync(userStorage.Current.LastGamePath, userStorage.Current.LastLanguage);
    }

  
    public void AddGameCode(string gamePath, string code, Language language)
    {
        var key = GetKey(gamePath, language);
        _codes.AddOrUpdate(key, code, (k, v) => code);
        _creatingFilesTasks.AddOrUpdate(key, Task.Run(async () =>
        {
            var path = GeneratePath(key);
            logger.LogDebug("Adding code to the path {path}", path);
            await File.WriteAllTextAsync(path, code);
            logger.LogInformation("Added successfully code to the path {path}", path);
        }), (_, task) => task);
    }
    
    public async ValueTask<string?> GetGameCodeAsync(string gamePath, Language language)
    {
       var key = GetKey(gamePath, language);
       if (_codes.TryGetValue(key, out var code))
       {
           logger.LogDebug("Got code from the memory key: {key}", key);
           return code;
       }
          
       return await LoadGameCodeAsync(gamePath, key, language);
    }

    private async Task<string?> LoadGameCodeAsync(string gamePath, string key, Language language)
    {
        var code = (string?)null;
        var path = GeneratePath(key); 
        if(File.Exists(path))
            code = await File.ReadAllTextAsync(path);


        if (code is not null)
        {
            _codes.AddOrUpdate(key, code, (k, v) => code);
            logger.LogDebug("Got code from the path: {path}", path);
        }
        else 
            logger.LogDebug("Cound not find code for the key {key} in the memory or in the path {path}", key, path);
        return code;
    }

    private string GetKey(string gamePath, Language language)
    {
        var extensions = language.GetExtension();
        var path = Path.ChangeExtension(gamePath, extensions);
        var fileName = Path.GetDirectoryName(path)!.TrimDirectorySeparator() + "-" + Path.GetFileName(path).TrimDirectorySeparator();
        return Path.Join(options.Value.GameCodeDataPath, fileName);
    }
    
    private string GeneratePath(string key)
    {
        return Path.Join(options.Value.GameCodeDataPath, key);
    }
    
    private async Task LoadLastGameCodeAsync(string lastGamePath, Language language)
    {
       await GetGameCodeAsync(lastGamePath, language);
    }
}