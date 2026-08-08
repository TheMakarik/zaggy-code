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
        await SystemUnderTests.Execute(GetCode("MoveUp"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("MoveRight"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("MoveDown"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("MoveLeft"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("FillCell"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("IsCellFilled"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("IsWallFromUp"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("IsWallFromDown"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("IsWallFromLeft"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("IsWallFromRight"), CancellationToken.None);

        // Assert
        A.CallTo(() => executor.IsWallFromRight()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeWritesToOutput_WritesExpectedText()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.Execute(GetCode("WriteOutput"), CancellationToken.None);

        // Assert
        output.ToString().Should().Contain(ExpectedOutputText);
    }

    [Fact]
    public async Task Execute_WhenCodeReadsFromInput_ReadsExpectedText()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var expectedInput = _fixture.Create<string>();
        var input = new StringReader(expectedInput);
        var output = new StringWriter();
        SystemUnderTests.SetExecutor(executor).RedirectIo(input, output).SetSpeed(ExecutionSpeed.X1);

        // Act
        await SystemUnderTests.Execute(GetCode("ReadInput"), CancellationToken.None);

        // Assert
        input.ReadToEnd().Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_WhenCodeRaisesDebugLineUpdated_RaisesDebugLineUpdatedEvent()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);
        DebugLineUpdatedEventArgs? capturedArgs = null;
        SystemUnderTests.DebugLineUpdated += (sender, args) => capturedArgs = args;

        // Act
        await SystemUnderTests.Execute(GetCode("DebugLineUpdated"), CancellationToken.None);

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.LineNumber.Should().BeGreaterThan(0);
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
        await SystemUnderTests.Execute(GetCode("CodeErrorOccurred"), CancellationToken.None);

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
        await SystemUnderTests.Execute(GetCode("MultipleLines"), cancellationTokenSource.Token);

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
        await SystemUnderTests.Execute(GetCode("MultipleLines"), CancellationToken.None);
        stopwatch.Stop();

        // Assert
        lineNumbers.Should().HaveCountGreaterThan(1);
        stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(10));
    }

    protected virtual string ExpectedOutputText => "expected output";

    protected (TextReader Input, StringWriter Output) ConfigureRunner(IRobotExecutor executor)
    {
        var input = new StringReader(string.Empty);
        var output = new StringWriter();
        SystemUnderTests.RedirectIo(input, output).SetSpeed(ExecutionSpeed.X10).SetExecutor(executor);
        return (input, output);
    }

    public void Dispose()
    {
        SystemUnderTests.Dispose();
    }
}
