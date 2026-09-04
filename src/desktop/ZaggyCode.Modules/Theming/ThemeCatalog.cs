namespace ZaggyCode.Modules.Theming;

public sealed class ThemeCatalog(
    ILogger<ThemeCatalog> logger,
    IArchiveReader archiveReader,
    IOptions<ThemeOptions> themeOptions,
    ITempFolderProvider tempFolderProvider,
    XmlSerializer<Theme> themeSerializer) : IThemeCatalog
{
    public async Task<IReadOnlyList<ThemeMetadata>> GetAvailableThemesAsync(CancellationToken token = default)
    {
        var systemThemesFolder = Path.GetFullPath(themeOptions.Value.SystemThemesFolder);
        var directories = GetExistingDirectories();
        if (directories.Count is 0)
        {
            logger.LogWarning("No theme directories found");
            return [];
        }

        var themes = new Dictionary<string, ThemeMetadata>(StringComparer.Ordinal);
        foreach (var directory in directories)
        {
            var isSystem = Path.GetFullPath(directory).Equals(systemThemesFolder, StringComparison.OrdinalIgnoreCase);
            foreach (var file in Directory.EnumerateFiles(directory, $"*{themeOptions.Value.ThemeExtensions}", SearchOption.TopDirectoryOnly))
            {
                var metadata = await archiveReader.ReadMetadataAsync<ThemeMetadata>(file);
                if (metadata is null)
                    continue;

                metadata.IsSystemTheme = isSystem;
                metadata.Path = file;
                themes[metadata.Name] = metadata;
            }
        }

        logger.LogInformation("Found {count} available themes", themes.Count);
        return themes.Values.ToList();
    }

    public async Task<Theme?> LoadThemeAsync(string name, CancellationToken token = default)
    {
        var archivePath = await FindThemeArchive(name);
        if (archivePath is null)
        {
            logger.LogWarning("Theme {name} was not found", name);
            return null;
        }

        var extractedDirectory = await archiveReader.ExtractAllToTempAsync(archivePath, new Progress<int>());
        try
        {
            var themeFile = Path.Join(extractedDirectory.FullName, themeOptions.Value.ThemeFileName);
            if (!File.Exists(themeFile))
            {
                logger.LogError("Theme {name} archive does not contain {file}", name, themeOptions.Value.ThemeFileName);
                return null;
            }

            await using var themeStream = File.OpenRead(themeFile);
            var theme = themeSerializer.Deserialize(themeStream);

            if (theme is null)
                logger.LogError("Failed to deserialize theme {name} from {file}", name, themeFile);

            return theme;
        }
        finally
        {
            Directory.Delete(extractedDirectory.FullName, recursive: true);
        }
    }

    private async Task<string?> FindThemeArchive(string name)
    {
        foreach (var directory in GetExistingDirectories())
        {
            foreach (var file in Directory.EnumerateFiles(directory, $"*{themeOptions.Value.ThemeExtensions}"))
            {
                var metadata = await archiveReader.ReadMetadataAsync<ThemeMetadata>(file);
                if (metadata?.Name == name)
                    return file;
            }
        }

        return null;
    }

    private List<string> GetExistingDirectories()
    {
        List<string> directories = [];
        AddIfExists(directories, themeOptions.Value.SystemThemesFolder);
        AddIfExists(directories, themeOptions.Value.ExternThemesFolder);
        return directories;
    }

    private void AddIfExists(List<string> directories, string directory)
    {
        if (Directory.Exists(directory))
            directories.Add(directory);
    }
}
