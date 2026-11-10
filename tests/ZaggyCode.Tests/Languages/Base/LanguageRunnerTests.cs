namespace ZaggyCode.Tests.Languages.Base;

public abstract class LanguageRunnerTests
{
    private readonly Fixture _fixture = new();

    protected abstract string CodeDirectory { get; }

    protected abstract ILanguageRunner CreateRunner(IRobotController controller, TextReader input, TextWriter output);

    protected abstract string GetCode(string propertyName);

    protected async Task<(IRobotController Controller, TextReader Input, TextWriter Output, List<ComputedLineOfCode> Lines)> ExecuteAsync(
        string code,
        IRobotController? controller = null,
        TextReader? input = null,
        TextWriter? output = null,
        CancellationToken cancellationToken = default)
    {
        controller ??= A.Fake<IRobotController>();
        input ??= A.Fake<TextReader>();
        output ??= A.Fake<TextWriter>();

        await using var runner = CreateRunner(controller, input, output);

        List<ComputedLineOfCode> lines = [];
        await foreach (var line in runner.ExecuteAsync(code, cancellationToken))
            lines.Add(line);

        return (controller, input, output, lines);
    }

    [Fact]
    public async Task Execute_WhenCodeCallsMoveUp_CallsMoveUpOnController()
    {
        var (controller, _, _, _) = await ExecuteAsync(GetCode("MoveUp"));

        A.CallTo(() => controller.MoveUp()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsMoveRight_CallsMoveRightOnController()
    {
        var (controller, _, _, _) = await ExecuteAsync(GetCode("MoveRight"));

        A.CallTo(() => controller.MoveRight()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsMoveDown_CallsMoveDownOnController()
    {
        var (controller, _, _, _) = await ExecuteAsync(GetCode("MoveDown"));

        A.CallTo(() => controller.MoveDown()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsMoveLeft_CallsMoveLeftOnController()
    {
        var (controller, _, _, _) = await ExecuteAsync(GetCode("MoveLeft"));

        A.CallTo(() => controller.MoveLeft()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsFillCell_CallsFillCellOnController()
    {
        var (controller, _, _, _) = await ExecuteAsync(GetCode("FillCell"));

        A.CallTo(() => controller.FillCell()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsCellFilled_CallsIsCellFilledOnController()
    {
        var controller = A.Fake<IRobotController>();
        A.CallTo(() => controller.IsCellFilled()).Returns(_fixture.Create<bool>());

        await ExecuteAsync(GetCode("IsCellFilled"), controller);

        A.CallTo(() => controller.IsCellFilled()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromUp_CallsIsWallFromUpOnController()
    {
        var controller = A.Fake<IRobotController>();
        A.CallTo(() => controller.IsWallFromUp()).Returns(_fixture.Create<bool>());

        await ExecuteAsync(GetCode("IsWallFromUp"), controller);

        A.CallTo(() => controller.IsWallFromUp()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromDown_CallsIsWallFromDownOnController()
    {
        var controller = A.Fake<IRobotController>();
        A.CallTo(() => controller.IsWallFromDown()).Returns(_fixture.Create<bool>());

        await ExecuteAsync(GetCode("IsWallFromDown"), controller);

        A.CallTo(() => controller.IsWallFromDown()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromLeft_CallsIsWallFromLeftOnController()
    {
        var controller = A.Fake<IRobotController>();
        A.CallTo(() => controller.IsWallFromLeft()).Returns(_fixture.Create<bool>());

        await ExecuteAsync(GetCode("IsWallFromLeft"), controller);

        A.CallTo(() => controller.IsWallFromLeft()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeCallsIsWallFromRight_CallsIsWallFromRightOnController()
    {
        var controller = A.Fake<IRobotController>();
        A.CallTo(() => controller.IsWallFromRight()).Returns(_fixture.Create<bool>());

        await ExecuteAsync(GetCode("IsWallFromRight"), controller);

        A.CallTo(() => controller.IsWallFromRight()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeWritesToOutput_CallsAnyMethodOnOutput()
    {
        var (_, _, output, _) = await ExecuteAsync(GetCode("WriteOutput"));

        A.CallTo(output).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeReadsFromInput_CallsAnyMethodOnInput()
    {
        var expectedInput = _fixture.Create<string>();
        var input = A.Fake<TextReader>();
        A.CallTo(() => input.ReadLine()).Returns(expectedInput);

        await ExecuteAsync(GetCode("ReadInput"), input: input);

        A.CallTo(input).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenCodeRaisesDebugLineUpdated_ReturnsLineUpdates()
    {
        var (_, _, _, lines) = await ExecuteAsync(GetCode("DebugLineUpdated"));

        lines.Should().Contain(line => line.LineNumber > 0);
    }

    [Fact]
    public async Task Execute_WhenCodeRaisesCodeErrorOccurred_ReturnsCodeError()
    {
        var (_, _, _, lines) = await ExecuteAsync(GetCode("CodeErrorOccurred"));

        var error = lines.Select(line => line.CodeError).FirstOrDefault(error => error.CodeErrorType != CodeErrorType.None);
        error.CodeErrorType.Should().NotBe(CodeErrorType.None);
        error.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Execute_WhenCancellationTokenIsCancelled_StopsExecution()
    {
        var controller = A.Fake<IRobotController>();
        var input = A.Fake<TextReader>();
        var output = A.Fake<TextWriter>();
        await using var runner = CreateRunner(controller, input, output);

        using var cancellationTokenSource = new CancellationTokenSource();
        List<ComputedLineOfCode> lines = [];

        await foreach (var line in runner.ExecuteAsync(GetCode("MultipleLines"), cancellationTokenSource.Token))
        {
            lines.Add(line);
            cancellationTokenSource.Cancel();
        }

        lines.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Execute_WithMultipleLines_ReturnsAllLines()
    {
        var (_, _, _, lines) = await ExecuteAsync(GetCode("MultipleLines"));

        lines.Count(line => line.LineNumber > 0).Should().BeGreaterThan(1);
    }
}
