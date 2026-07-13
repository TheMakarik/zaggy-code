using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLua.Exceptions;
using Xunit;
using ZaggyCode.Core.Data;
using ZaggyCode.Core.Data.Interfaces;
using ZaggyCode.Core.Data.Model;
using ZaggyCode.Core.Data.Options;
using ZaggyCode.Core.Languages.Enums;
using ZaggyCode.Core.Languages.Exceptions;
using ZaggyCode.Core.Languages.Lua;
using ZaggyCode.Core.Languages.Options;

namespace ZaggyCode.Data.Tests.Languages;

public sealed class LuaLanguageRunnerTests : IDisposable
{
    private const string CodeWithRobotMethods = @"
robot.move_up()
robot.can_move_right()
robot.draw()
";

    private const string CodeWithMultipleLines = @"
local a = 1
local b = 2
local c = a + b
print(c)
";

    private const string CodeWithSyntaxError = "syntax error";

    private const string CodeWithIoWrite = @"
io.write(""Hello"")
io.write("" World"")
";

    private const string CodeWithPrint = @"
print(""First"")
print(""Second"")
";

    private const string CodeWithIoRead = @"
local line = io.read()
print(line)
";

    private readonly IServiceProvider _serviceProvider;
    private readonly Fixture _fixture;

    public LuaLanguageRunnerTests()
    {
        _fixture = new Fixture();
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
            
        var services = new ServiceCollection();
        services.Configure<SpeedMillisecondsOptions>(configuration.GetSection(nameof(SpeedMillisecondsOptions)));
        services.Configure<LuaPathsOptions>(configuration.GetSection(nameof(LuaPathsOptions)));
        
        _serviceProvider = services.BuildServiceProvider();
    }

    private LuaLanguageRunner CreateSystemUnderTests(ILogger<LuaLanguageRunner> logger, IUserStorage userStorage)
    {
        var speedOptions = _serviceProvider.GetRequiredService<IOptions<SpeedMillisecondsOptions>>();
        var luaOptions = _serviceProvider.GetRequiredService<IOptions<LuaPathsOptions>>();
        return new LuaLanguageRunner(userStorage, logger, speedOptions, luaOptions);
    }

    private IUserStorage CreateUserStorageWithChecker(bool useTableContentChecker)
    {
        var userStorage = A.Fake<IUserStorage>();
        var userData = _fixture.Build<UserData>()
            .With(x => x.LuaData, new LuaData
            {
                MaxRecursion = 100,
                UseTableContentCheckerByDefault = useTableContentChecker
            })
            .Create();
        A.CallTo(() => userStorage.Current).Returns(userData);
        return userStorage;
    }

    [Fact]
    public void Execute_WhenRobotMethodsCalled_ShouldInvokeCorrespondingExecutorMethods()
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: false);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var executor = A.Fake<IRobotExecutor>();

        // Act
        systemUnderTests.Execute(CodeWithRobotMethods, ExecutionSpeed.X1, executor);

        // Assert
        A.CallTo(() => executor.MoveUp()).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.CanMoveRight()).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.Draw()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_WhenDebugHookActive_ShouldRaiseDebugLineUpdatedForEachLine()
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: false);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var executor = A.Fake<IRobotExecutor>();

        var lineNumbers = new List<int>();
        systemUnderTests.DebugLineUpdated += (_, args) => lineNumbers.Add(args.LineNumber);

        // Act
        systemUnderTests.Execute(CodeWithMultipleLines, ExecutionSpeed.X1, executor);

        // Assert
        lineNumbers.Should().NotBeEmpty();
        lineNumbers.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void Execute_WhenSyntaxError_ShouldRaiseCodeErrorOccurred()
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: false);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var executor = A.Fake<IRobotExecutor>();

        var errorRaised = false;
        systemUnderTests.CodeErrorOccurred += (_, args) =>
        {
            errorRaised = true;
            args.Text.Should().NotBeNull();
        };

        // Act
        systemUnderTests.Execute(CodeWithSyntaxError, ExecutionSpeed.X1, executor);

        // Assert
        errorRaised.Should().BeTrue();
    }

    [Theory]
    [InlineData("prinf", "print")]
    [InlineData("dehug", "debug")]
    [InlineData("_VERSIO", "_VERSION")]
    [InlineData("io.writ", "io.write")]
    [InlineData("debug.setbook", "debug.sethook")]
    public void Execute_WhenTableContentCheckerEnabledAndTypoExists_ShouldThrowLuaIncorrectlyWroteNameException(
        string actualName, 
        string expectedSuggestion)
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: true);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var executor = A.Fake<IRobotExecutor>();
        var code = $@"
{actualName} = 42
";

        // Act
        var exception = Record.Exception(() => systemUnderTests.Execute(code, ExecutionSpeed.X1, executor));

        // Assert
        exception.Should().BeOfType<LuaIncorrectlyWroteNameException>();
        var typed = exception as LuaIncorrectlyWroteNameException;
        typed!.Actual.Should().Be(actualName);
        typed.Suggestion.Should().Be(expectedSuggestion);
    }

    [Theory]
    [InlineData("prinf", "print")]
    [InlineData("dehug", "debug")]
    public void Execute_WhenTableContentCheckerDisabledAndTypoExists_ShouldNotThrow(
        string actualName,
        string expectedSuggestion)
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: false);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var executor = A.Fake<IRobotExecutor>();
        var code = $@"
{actualName} = 42
";

        // Act
        var exception = Record.Exception(() => systemUnderTests.Execute(code, ExecutionSpeed.X1, executor));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void RedirectIo_ShouldMakeIoWriteCallTextWriterWrite()
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: false);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var textWriter = A.Fake<TextWriter>();
        var textReader = A.Fake<TextReader>();
        systemUnderTests.RedirectIo(textReader, textWriter);

        var executor = A.Fake<IRobotExecutor>();

        // Act
        systemUnderTests.Execute(CodeWithIoWrite, ExecutionSpeed.X1, executor);

        // Assert
        A.CallTo(() => textWriter.Write(A<string>._)).MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public void RedirectIo_ShouldMakePrintCallTextWriterWriteLine()
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: false);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var textWriter = A.Fake<TextWriter>();
        var textReader = A.Fake<TextReader>();
        systemUnderTests.RedirectIo(textReader, textWriter);

        var executor = A.Fake<IRobotExecutor>();

        // Act
        systemUnderTests.Execute(CodeWithPrint, ExecutionSpeed.X1, executor);

        // Assert
        A.CallTo(() => textWriter.WriteLine(A<string>._)).MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public void RedirectIo_ShouldMakeIoReadCallTextReaderReadLine()
    {
        // Arrange
        var logger = A.Fake<ILogger<LuaLanguageRunner>>();
        var userStorage = CreateUserStorageWithChecker(useTableContentChecker: false);
        
        using var systemUnderTests = CreateSystemUnderTests(logger, userStorage);
        var textWriter = A.Fake<TextWriter>();
        var textReader = A.Fake<TextReader>();
        var expectedInput = _fixture.Create<string>();
        A.CallTo(() => textReader.ReadLine()).Returns(expectedInput);
        systemUnderTests.RedirectIo(textReader, textWriter);

        var executor = A.Fake<IRobotExecutor>();

        // Act
        systemUnderTests.Execute(CodeWithIoRead, ExecutionSpeed.X1, executor);

        // Assert
        A.CallTo(() => textReader.ReadLine()).MustHaveHappenedOnceExactly();
        A.CallTo(() => textWriter.WriteLine(expectedInput)).MustHaveHappenedOnceExactly();
    }

    public void Dispose()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }
}