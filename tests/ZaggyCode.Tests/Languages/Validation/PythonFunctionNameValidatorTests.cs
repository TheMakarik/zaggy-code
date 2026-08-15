using ZaggyCode.Modules.Languages.Validation;

namespace ZaggyCode.Tests.Languages.Validation;

public sealed class PythonFunctionNameValidatorTests : IDisposable
{
    private readonly TestFileSystem _fileSystem = new();
    private readonly IOptions<PythonValidationOptions> _options;
    private readonly IOptions<PythonScriptsOptions> _scriptsOptions;

    public PythonFunctionNameValidatorTests()
    {
        _options = A.Fake<IOptions<PythonValidationOptions>>();
        A.CallTo(() => _options.Value).Returns(new PythonValidationOptions
        {
            ForbiddenCharacters = ["@", "#", "$"],
            GlobalFunctions = ["print", "input"],
            SkipFiles = ["__init__.py"]
        });

        _scriptsOptions = A.Fake<IOptions<PythonScriptsOptions>>();
        A.CallTo(() => _scriptsOptions.Value).Returns(new PythonScriptsOptions
        {
            PrepareModules = string.Empty,
            RedirectIoPath = string.Empty,
            SetLineUpdatingPath = string.Empty,
            RobotPath = string.Empty,
            StandardLibraryPath = _fileSystem.RootPath,
            DisableLineUpdating = string.Empty
        });
        
        CreateFakeStdLibFile("os.py", "def path():\n    pass\n");
        CreateFakeStdLibFile("__init__.py", "def ignored():\n    pass\n");
    }

    public void Dispose()
    {
        _fileSystem.Dispose();
    }

    [Theory]
    [InlineData("myFunction")]
    [InlineData("main")]
    [InlineData("run_code")]
    public void Validate_WhenNameIsValid_ReturnsSuccess(string functionName)
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate(functionName);

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.Success);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ReturnsEmpty()
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate(string.Empty);

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.Empty);
    }

    [Fact]
    public void Validate_WhenNameContainsSpaces_ReturnsContainsSpaces()
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate("my function");

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.ContainsSpaces);
    }

    [Theory]
    [InlineData("my@function")]
    [InlineData("my#function")]
    [InlineData("my$function")]
    public void Validate_WhenNameContainsForbiddenCharacters_ReturnsContainsForbiddenCharacters(string functionName)
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate(functionName);

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.ContainsForbiddenCharacters);
    }

    [Fact]
    public void Validate_WhenNameStartsWithDigit_ReturnsStartsWithDigit()
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate("1function");

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.StartsWithDigit);
    }

    [Fact]
    public void Validate_WhenNameStartsWithUnderscore_ReturnsStartsWithUnderscore()
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate("_function");

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.StartsWithUnderscore);
    }

    [Theory]
    [InlineData("print")]
    [InlineData("input")]
    public void Validate_WhenNameIsReservedGlobalFunction_ReturnsIsReservedGlobalFunction(string functionName)
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate(functionName);

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.IsReservedGlobalFunction);
    }

    [Fact]
    public void Validate_WhenNameMatchesStandardLibraryModule_ReturnsIsStandardLibraryModule()
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate("os");

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.IsStandardLibraryModule);
    }

    [Fact]
    public void Validate_WhenNameMatchesStandardLibraryFunction_ReturnsIsStandardLibraryFunction()
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate("path");

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.IsStandardLibraryFunction);
    }

    [Fact]
    public void Validate_WhenNameMatchesSkippedFileFunction_ReturnsSuccess()
    {
        // Arrange
        var systemUnderTests = CreateValidator();

        // Act
        var result = systemUnderTests.Validate("ignored");

        // Assert
        result.Should().Be(PythonFunctionNameValidationResult.Success);
    }

    private IPythonFunctionNameValidator CreateValidator()
    {
        return new PythonFunctionNameValidator(_options, _scriptsOptions, A.Dummy<ILogger<PythonFunctionNameValidator>>());
    }

    private void CreateFakeStdLibFile(string fileName, string content)
    {
        var path = Path.Join(_fileSystem.RootPath, fileName);
        File.WriteAllText(path, content);
    }
}
