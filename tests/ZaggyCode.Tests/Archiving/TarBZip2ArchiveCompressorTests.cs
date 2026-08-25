namespace ZaggyCode.Tests.Archiving;

public class TarBZip2ArchiveCompressorTests : IDisposable
{
    private const string ArchiveExtension = ".tar.bz2";

    private readonly Fixture _fixture = new();
    private readonly TestFileSystem _fileSystem = new();
    private readonly IOptions<TempOptions> _tempOptions;
    private readonly TarBZip2ArchiveCompressor _systemUnderTests;
    private readonly TarBZip2ArchiveReader _reader;

    public TarBZip2ArchiveCompressorTests()
    {
        _tempOptions = CreateTempOptions();
        _systemUnderTests = new TarBZip2ArchiveCompressor(
            A.Dummy<ILogger<TarBZip2ArchiveCompressor>>(),
            _tempOptions);
        _reader = new TarBZip2ArchiveReader(
            A.Dummy<ILogger<TarBZip2ArchiveReader>>(),
            new MetadataParser(CreateMetadataOptions()),
            _tempOptions);
    }

    [Fact]
    public async Task CompressAsync_WhenFolderInsideToCompress_CreatesValidArchive()
    {
        // Arrange
        var expectedContent = _fixture.Create<string>();
        var sourceDirectory = CreateToCompressFolder("source");
        var filePath = Path.Join(sourceDirectory, "file.txt");
        await File.WriteAllTextAsync(filePath, expectedContent);
        var archivePath = Path.Join(_fileSystem.CreateDirectory(), "archive" + ArchiveExtension);

        // Act
        await _systemUnderTests.CompressAsync(archivePath, sourceDirectory);

        // Assert
        File.Exists(archivePath).Should().BeTrue();
        var extractedDirectory = await _reader.ExtractAllToTempAsync(archivePath, new Progress<int>());
        var extractedFiles = Directory.GetFiles(extractedDirectory.FullName, "*", SearchOption.AllDirectories);
        extractedFiles.Should().ContainSingle();
        (await File.ReadAllTextAsync(extractedFiles[0])).Should().Be(expectedContent);
    }

    [Fact]
    public async Task CompressAsync_WhenNestedFolders_PreservesRelativePaths()
    {
        // Arrange
        var expectedContent = _fixture.Create<string>();
        var sourceDirectory = CreateToCompressFolder("nested-source");
        var nestedFilePath = Path.Join(sourceDirectory, "inner", "file.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(nestedFilePath)!);
        await File.WriteAllTextAsync(nestedFilePath, expectedContent);
        var archivePath = Path.Join(_fileSystem.CreateDirectory(), "nested" + ArchiveExtension);

        // Act
        await _systemUnderTests.CompressAsync(archivePath, sourceDirectory);

        // Assert
        var extractedDirectory = await _reader.ExtractAllToTempAsync(archivePath, new Progress<int>());
        var nestedExtractedFile = Path.Join(extractedDirectory.FullName, "inner", "file.txt");
        File.Exists(nestedExtractedFile).Should().BeTrue();
        (await File.ReadAllTextAsync(nestedExtractedFile)).Should().Be(expectedContent);
    }

    [Fact]
    public void CompressAsync_WhenFolderOutsideToCompress_ThrowsArgumentException()
    {
        // Arrange
        var outsideDirectory = _fileSystem.CreateDirectory();
        var archivePath = Path.Join(outsideDirectory, "archive" + ArchiveExtension);

        // Act
        var act = () => _systemUnderTests.CompressAsync(archivePath, outsideDirectory);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CompressAsync_WhenCalled_ExtractsIntoFromCompressDirectory()
    {
        // Arrange
        var sourceDirectory = CreateToCompressFolder("extract-target");
        await File.WriteAllTextAsync(Path.Join(sourceDirectory, "file.txt"), _fixture.Create<string>());
        var archivePath = Path.Join(_fileSystem.CreateDirectory(), "archive" + ArchiveExtension);
        await _systemUnderTests.CompressAsync(archivePath, sourceDirectory);

        // Act
        var extractedDirectory = await _reader.ExtractAllToTempAsync(archivePath, new Progress<int>());

        // Assert
        var fromCompressRoot = Path.GetFullPath(_tempOptions.Value.TempFromCompress);
        Path.GetFullPath(extractedDirectory.FullName)
            .Should().StartWith(fromCompressRoot);
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }

    private string CreateToCompressFolder(string name)
    {
        var directory = Path.Join(_tempOptions.Value.TempToCompress, name);
        Directory.CreateDirectory(directory);
        return directory;
    }

    private IOptions<TempOptions> CreateTempOptions()
    {
        var options = A.Fake<IOptions<TempOptions>>();
        A.CallTo(() => options.Value).Returns(new TempOptions
        {
            TempDirectoryPath = _fileSystem.CreateDirectory(),
            TempToCompress = _fileSystem.CreateDirectory(),
            TempFromCompress = _fileSystem.CreateDirectory()
        });
        return options;
    }

    private static IOptions<MetadataOptions> CreateMetadataOptions()
    {
        var options = A.Fake<IOptions<MetadataOptions>>();
        A.CallTo(() => options.Value).Returns(new MetadataOptions { MetadataFile = "meta.json" });
        return options;
    }
}
