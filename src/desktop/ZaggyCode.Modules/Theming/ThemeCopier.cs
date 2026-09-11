namespace ZaggyCode.Modules.Theming;

public sealed class ThemeCopier(
    ILogger<ThemeCopier> logger,
    IArchiveReader archiveReader,
    IArchiveCompressor compressor,
    ITempFolderProvider tempFolderProvider,
    IOptions<ThemeOptions> themeOptions,
    IOptions<MetadataOptions> metadataOptions) : IThemeCopier
{
    public async Task<string> CopyThemeAsync(ThemeMetadata theme, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(theme.Path) || !File.Exists(theme.Path))
            throw new FileNotFoundException($"Theme archive '{theme.Name}' was not found", theme.Path);

        var copyName = BuildCopyName(theme.Name);
        var targetPath = Path.Join(themeOptions.Value.ExternThemesFolder, $"{copyName}{themeOptions.Value.ThemeExtensions}");
        Directory.CreateDirectory(themeOptions.Value.ExternThemesFolder);

        var extractedDirectory = await archiveReader.ExtractAllToTempAsync(theme.Path, new Progress<int>());
        try
        {
            await WriteCopyArchiveAsync(extractedDirectory.FullName, theme, copyName, targetPath, token);
        }
        finally
        {
            Directory.Delete(extractedDirectory.FullName, recursive: true);
        }

        logger.LogInformation("Copied theme '{name}' to {path}", theme.Name, targetPath);
        return targetPath;
    }

    private async Task WriteCopyArchiveAsync(string extractedDirectory, ThemeMetadata source, string copyName, string targetPath, CancellationToken token)
    {
        var stagingDirectory = Path.Join(tempFolderProvider.GetToCompressPath(), copyName);
        if (Directory.Exists(stagingDirectory))
            Directory.Delete(stagingDirectory, recursive: true);
        Directory.CreateDirectory(stagingDirectory);

        var themeFile = Path.Join(extractedDirectory, themeOptions.Value.ThemeFileName);
        if (File.Exists(themeFile))
            File.Copy(themeFile, Path.Join(stagingDirectory, themeOptions.Value.ThemeFileName), overwrite: true);

        await using (var metadataStream = File.Create(Path.Join(stagingDirectory, metadataOptions.Value.MetadataFile)))
        {
            await JsonSerializer.SerializeAsync(metadataStream, BuildCopyMetadata(source, copyName));
        }

        await compressor.CompressAsync(targetPath, stagingDirectory);
    }

    private static ThemeMetadata BuildCopyMetadata(ThemeMetadata source, string copyName) => new()
    {
        Name = copyName,
        Author = source.Author,
        CreatedAtVersion = source.CreatedAtVersion,
        Description = source.Description,
        BackgroundColor = source.BackgroundColor,
        SidebarBackgroundColor = source.SidebarBackgroundColor,
        EditorBackgroundColor = source.EditorBackgroundColor,
        TerminalBackgroundColor = source.TerminalBackgroundColor,
        PrimaryColor = source.PrimaryColor,
        BorderColor = source.BorderColor,
        ForegroundColor = source.ForegroundColor
    };

    private string BuildCopyName(string name)
    {
        var candidate = $"{name} - копия";
        var index = 1;
        while (ThemeExists(candidate))
        {
            candidate = $"{name} - копия ({index})";
            index++;
        }

        return candidate;
    }

    private bool ThemeExists(string copyName)
        => File.Exists(Path.Join(themeOptions.Value.ExternThemesFolder, $"{copyName}{themeOptions.Value.ThemeExtensions}"));
}
