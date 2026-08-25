namespace ZaggyCode.Modules.Theming;

public sealed class ThemeCreator(
    ILogger<ThemeCreator> logger,
    IArchiveCompressor compressor,
    ITempFolderProvider tempFolderProvider,
    IOptions<ThemeOptions> themeOptions,
    IOptions<MetadataOptions> metadataOptions,
    XmlSerializer<Theme> themeSerializer) : IThemeCreator
{
    public async Task CreateAsync(Theme theme, ThemeMetadata metadata, string outputDirectory)
    {
        var stagingDirectory = Path.Join(tempFolderProvider.GetToCompressPath(), metadata.Name);
        if (Directory.Exists(stagingDirectory))
            Directory.Delete(stagingDirectory, recursive: true);

        Directory.CreateDirectory(stagingDirectory);

        try
        {
            await WriteThemeFilesAsync(theme, metadata, stagingDirectory);

            Directory.CreateDirectory(outputDirectory);
            var archivePath = Path.Join(outputDirectory, $"{metadata.Name}{themeOptions.Value.ThemeExtensions}");
            await compressor.CompressAsync(archivePath, stagingDirectory);

            logger.LogInformation("Created theme archive {archive} for theme {name}", archivePath, metadata.Name);
        }
        finally
        {
            Directory.Delete(stagingDirectory, recursive: true);
        }
    }

    private async Task WriteThemeFilesAsync(Theme theme, ThemeMetadata metadata, string stagingDirectory)
    {
        await using (var themeStream = File.Create(Path.Join(stagingDirectory, themeOptions.Value.ThemeFileName)))
        {
            themeSerializer.Serialize(themeStream, theme);
        }

        await using (var metadataStream = File.Create(Path.Join(stagingDirectory, metadataOptions.Value.MetadataFile)))
        {
            await JsonSerializer.SerializeAsync(metadataStream, metadata);
        }
    }
}
