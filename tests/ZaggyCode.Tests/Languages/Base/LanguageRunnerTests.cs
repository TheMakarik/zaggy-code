namespace ZaggyCode.Tests.Languages.Base;

public abstract class LanguageRunnerTests : IDisposable
{
    private readonly Fixture _fixture = new();

    protected abstract ILanguageRunner SystemUnderTests { get; }

    protected abstract  string CodeDirectory { get; }

    protected abstract string GetCode(string propertyName);
    [Fact]
    public async Task Execute_WhenCodeCallsMoveUp_CallsMoveUpOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("MoveUp"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.MoveUp()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsMoveRight_CallsMoveRightOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("MoveRight"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.MoveRight()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsMoveDown_CallsMoveDownOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("MoveDown"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.MoveDown()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsMoveLeft_CallsMoveLeftOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("MoveLeft"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.MoveLeft()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsFillCell_CallsFillCellOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("FillCell"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.FillCell()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsCellFilled_CallsIsCellFilledOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        A.CallTo(() => executor.IsCellFilled()).Returns(_fixture.Create<bool>());
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("IsCellFilled"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.IsCellFilled()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromUp_CallsIsWallFromUpOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        A.CallTo(() => executor.IsWallFromUp()).Returns(_fixture.Create<bool>());
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("IsWallFromUp"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.IsWallFromUp()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromDown_CallsIsWallFromDownOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        A.CallTo(() => executor.IsWallFromDown()).Returns(_fixture.Create<bool>());
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("IsWallFromDown"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.IsWallFromDown()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromLeft_CallsIsWallFromLeftOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        A.CallTo(() => executor.IsWallFromLeft()).Returns(_fixture.Create<bool>());
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("IsWallFromLeft"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.IsWallFromLeft()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromRight_CallsIsWallFromRightOnExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        A.CallTo(() => executor.IsWallFromRight()).Returns(_fixture.Create<bool>());
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("IsWallFromRight"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.IsWallFromRight()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeWritesToOutput_CallsAnyMethodOnOutput()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("WriteOutput"), CancellationToken.None);

        // Assert
        A.CallTo(output).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeReadsFromInput_CallsAnyMethodOnInput()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var expectedInput = _fixture.Create<string>();
        var (input, output) = ConfigureRunner(executor);
        A.CallTo(input).WithReturnType<string>().Returns(expectedInput);

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("ReadInput"), CancellationToken.None);

        // Assert
        A.CallTo(input).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeRaisesDebugLineUpdated_RaisesDebugLineUpdatedEvent()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);
        var wasCalled = false;
        SystemUnderTests.DebugLineUpdated += (sender, args) => wasCalled = true;

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("DebugLineUpdated"), CancellationToken.None);

        // Assert 
        wasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WhenCodeRaisesCodeErrorOccurred_RaisesCodeErrorOccurredEvent()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);
        CodeErrorOccurredEventArgs? capturedArgs = null;
        SystemUnderTests.CodeErrorOccurred += (sender, args) => capturedArgs = args;

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("CodeErrorOccurred"), CancellationToken.None);

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Text.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Execute_WhenCancellationTokenIsCancelled_StopsExecution()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);
        using var cancellationTokenSource = new CancellationTokenSource();
        var eventRaised = false;

        SystemUnderTests.DebugLineUpdated += (_, _) =>
        {
            eventRaised = true;
            cancellationTokenSource.Cancel();
        };

        // Act
        await SystemUnderTests.ExecuteAsync(GetCode("MultipleLines"), cancellationTokenSource.Token);

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_WithSpeed_UpdatesLinesWithDelay()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);
        var lineNumbers = new List<int>();

        SystemUnderTests.DebugLineUpdated += (_, args) => lineNumbers.Add(args.LineNumber);

        // Act
        var stopwatch = Stopwatch.StartNew();
        await SystemUnderTests.ExecuteAsync(GetCode("MultipleLines"), CancellationToken.None);
        stopwatch.Stop();

        // Assert
        lineNumbers.Should().HaveCountGreaterThan(1);
        stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(10));
    }

    protected (TextReader Input, TextWriter Output) ConfigureRunner(IRobotExecutor executor)
    {
        var input = A.Fake<TextReader>();
        var output = A.Fake<TextWriter>();
        SystemUnderTests.RedirectIo(input, output, CancellationToken.None);
        SystemUnderTests.SetExecutor(executor, CancellationToken.None);
        SystemUnderTests.SetSpeed(ExecutionSpeed.X10, CancellationToken.None);
        return (input, output);
    }

    public void Dispose()
    {
        SystemUnderTests.Dispose();
    }
}
