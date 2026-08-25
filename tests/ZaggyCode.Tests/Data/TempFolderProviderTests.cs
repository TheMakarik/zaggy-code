namespace ZaggyCode.Tests.Data;

public class TempFolderProviderTests : IDisposable
{
    private readonly Fixture _fixture = new();
    private readonly TestFileSystem _fileSystem = new();

    [Fact]
    public void GetTempPath_WhenDirectoryMissing_CreatesAndReturnsIt()
    {
        // Arrange
        var expectedPath = Path.Join(_fileSystem.CreateDirectory(), "temp");
        var systemUnderTests = CreateSystemUnderTests(expectedPath, _fixture.Create<string>(), _fixture.Create<string>());

        // Act
        var actualPath = systemUnderTests.GetTempPath();

        // Assert
        actualPath.Should().Be(expectedPath);
        Directory.Exists(actualPath).Should().BeTrue();
    }

    [Fact]
    public void GetToCompressPath_WhenDirectoryMissing_CreatesAndReturnsIt()
    {
        // Arrange
        var expectedPath = Path.Join(_fileSystem.CreateDirectory(), "to-compress");
        var systemUnderTests = CreateSystemUnderTests(_fixture.Create<string>(), expectedPath, _fixture.Create<string>());

        // Act
        var actualPath = systemUnderTests.GetToCompressPath();

        // Assert
        actualPath.Should().Be(expectedPath);
        Directory.Exists(actualPath).Should().BeTrue();
    }

    [Fact]
    public void GetFromCompressPath_WhenDirectoryMissing_CreatesAndReturnsIt()
    {
        // Arrange
        var expectedPath = Path.Join(_fileSystem.CreateDirectory(), "from-compress");
        var systemUnderTests = CreateSystemUnderTests(_fixture.Create<string>(), _fixture.Create<string>(), expectedPath);

        // Act
        var actualPath = systemUnderTests.GetFromCompressPath();

        // Assert
        actualPath.Should().Be(expectedPath);
        Directory.Exists(actualPath).Should().BeTrue();
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }

    private TempFolderProvider CreateSystemUnderTests(string tempPath, string toCompressPath, string fromCompressPath)
    {
        var options = A.Fake<IOptions<TempOptions>>();
        A.CallTo(() => options.Value).Returns(new TempOptions
        {
            TempDirectoryPath = tempPath,
            TempToCompress = toCompressPath,
            TempFromCompress = fromCompressPath
        });

        return new TempFolderProvider(options);
    }
}
