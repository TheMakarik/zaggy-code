namespace ZaggyCode.Tests.Archiving;

public class TarBZip2ArchiveReaderTests : IDisposable
{
    private const string MetadataFileName = "metadata.xml";
    private const string ArchiveExtension = ".tar.bz2";

    private readonly Fixture _fixture = new();
    private readonly TestFileSystem _fileSystem = new();
    private readonly TarBZip2ArchiveReader _systemUnderTests;

    public TarBZip2ArchiveReaderTests()
    {
        var logger = A.Dummy<ILogger<TarBZip2ArchiveReader>>();
        var metadataParser = new MetadataParser(CreateServiceProvider(), CreateMetadataOptions());
        var tempOptions = CreateTempOptions();
        _systemUnderTests = new TarBZip2ArchiveReader(logger, metadataParser, tempOptions);
    }

    [Fact]
    public async Task EnumerateMetadata_WhenFileHasWrongExtension_IgnoresFile()
    {
        // Arrange
        var directory = _fileSystem.CreateDirectory();
        CreateValidArchive(directory, "valid" + ArchiveExtension);
        CreateValidArchive(directory, "invalid" + ".zip");

        // Act
        var result = await _systemUnderTests
            .EnumerateMetadata<ArchiveMetadata>([directory], ArchiveExtension, recursive: false)
            .ToListAsync();

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("valid");
    }

    [Fact]
    public async Task EnumerateMetadata_WhenRecursiveIsFalse_SearchesOnlyTopDirectory()
    {
        // Arrange
        var directory = _fileSystem.CreateDirectory();
        CreateValidArchive(directory, "top" + ArchiveExtension);
        var subdirectory = Path.Join(directory, "sub");
        Directory.CreateDirectory(subdirectory);
        CreateValidArchive(subdirectory, "nested" + ArchiveExtension);

        // Act
        var result = await _systemUnderTests
            .EnumerateMetadata<ArchiveMetadata>([directory], ArchiveExtension, recursive: false)
            .ToListAsync();

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("top");
    }

    [Fact]
    public async Task EnumerateMetadata_WhenRecursiveIsTrue_SearchesSubdirectories()
    {
        // Arrange
        var directory = _fileSystem.CreateDirectory();
        CreateValidArchive(directory, "top" + ArchiveExtension);
        var subdirectory = Path.Join(directory, "sub");
        Directory.CreateDirectory(subdirectory);
        CreateValidArchive(subdirectory, "nested" + ArchiveExtension);

        // Act
        var result = await _systemUnderTests
            .EnumerateMetadata<ArchiveMetadata>([directory], ArchiveExtension, recursive: true)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(metadata => metadata.Name).Should().Contain("top", "nested");
    }

    [Fact]
    public async Task EnumerateMetadata_WhenMultipleArchivesExist_ReturnsAllMetadata()
    {
        // Arrange
        var directory = _fileSystem.CreateDirectory();
        CreateValidArchive(directory, "first" + ArchiveExtension, "First archive");
        CreateValidArchive(directory, "second" + ArchiveExtension, "Second archive");

        // Act
        var result = await _systemUnderTests
            .EnumerateMetadata<ArchiveMetadata>([directory], ArchiveExtension, recursive: false)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(metadata => metadata.Name).Should().Contain("first", "second");
    }

    [Fact]
    public async Task ExtractAllToTempAsync_WhenArchiveContainsFiles_ExtractsAllContent()
    {
        // Arrange
        var directory = _fileSystem.CreateDirectory();
        var expectedContent = _fixture.Create<string>();
        var archivePath = CreateArchive(directory, "archive" + ArchiveExtension,
        [
            ("file.txt", expectedContent),
            ("nested/file.txt", expectedContent)
        ]);
        var progress = A.Fake<IProgress<int>>();

        // Act
        var extractedDirectory = await _systemUnderTests.ExtractAllToTempAsync(archivePath, progress);

        // Assert
        var extractedFiles = Directory.GetFiles(extractedDirectory.FullName, "*", SearchOption.AllDirectories);
        extractedFiles.Should().HaveCount(2);
        foreach (var file in extractedFiles)
            File.ReadAllText(file).Should().Be(expectedContent);
    }

    [Fact]
    public async Task ExtractAllToTempAsync_WhenExtracting_ReportsProgress()
    {
        // Arrange
        var directory = _fileSystem.CreateDirectory();
        var archivePath = CreateArchive(directory, "archive" + ArchiveExtension,
        [
            ("file1.txt", _fixture.Create<string>()),
            ("file2.txt", _fixture.Create<string>())
        ]);
        var progress = A.Fake<IProgress<int>>();

        // Act
        await _systemUnderTests.ExtractAllToTempAsync(archivePath, progress);

        // Assert
        A.CallTo(() => progress.Report(An<int>.Ignored)).MustHaveHappened();
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(XmlSerializer<>), typeof(XmlSerializer<>));
        return services.BuildServiceProvider();
    }

    private static IOptions<MetadataOptions> CreateMetadataOptions()
    {
        var options = A.Fake<IOptions<MetadataOptions>>();
        A.CallTo(() => options.Value).Returns(new MetadataOptions { MetadataFile = MetadataFileName });
        return options;
    }

    private IOptions<TempOptions> CreateTempOptions()
    {
        var options = A.Fake<IOptions<TempOptions>>();
        A.CallTo(() => options.Value).Returns(new TempOptions { TempDirectoryPath = _fileSystem.CreateDirectory() });
        return options;
    }

    private void CreateValidArchive(string directory, string fileName, string? description = null)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        if (Path.GetExtension(name) == ".tar")
            name = Path.GetFileNameWithoutExtension(name);

        var metadata = new ArchiveMetadata
        {
            Name = name,
            Description = description,
            CreatedAtVersion = new Version(1, 0)
        };

        using var metadataStream = new MemoryStream();
        new XmlSerializer<ArchiveMetadata>().Serialize(metadataStream, metadata);

        CreateArchive(directory, fileName, [(MetadataFileName, metadataStream.ToArray())]);
    }

    private static string CreateArchive(string directory, string fileName, IReadOnlyList<(string EntryName, string Content)> entries)
    {
        var binaryEntries = entries.Select(entry => (entry.EntryName, Encoding.UTF8.GetBytes(entry.Content))).ToList();
        return CreateArchive(directory, fileName, binaryEntries);
    }

    private static string CreateArchive(string directory, string fileName, IReadOnlyList<(string EntryName, byte[] Content)> entries)
    {
        var archivePath = Path.Join(directory, fileName);
        using var fileStream = File.OpenWrite(archivePath);
        using var writer = WriterFactory.OpenWriter(fileStream, ArchiveType.Tar, new TarWriterOptions(CompressionType.BZip2, true));
        foreach (var (entryName, content) in entries)
        {
            using var entryStream = new MemoryStream(content);
            writer.Write(entryName, entryStream, modificationTime: null);
        }

        return archivePath;
    }
}
