using ZaggyCode.Modules.LanguageRuntime.Python;
using ZaggyCode.Tests.Languages.Base;

namespace ZaggyCode.Tests.Languages;

public class PythonLanguageRunnerTests : LanguageRunnerTests
{
    private readonly IOptions<PythonScriptsOptions> _pythonOptions;
    private readonly ISettingsStorage<PythonSettings> _pythonSettingsStorage;
    private PythonSettings _pythonSettings = new()
    {
        UseEntryFunction = false,
        EntryFunctionName = "main",
        SupressIo = false
    };

    public PythonLanguageRunnerTests()
    {
        SetEnvironmentVariables();

        var appDirectory = AppContext.BaseDirectory;
        var pythonDirectory = Path.Join(appDirectory, "LanguageRuntime", "Python");

        _pythonOptions = Options.Create(new PythonScriptsOptions
        {
            PrepareModules = Path.Join(pythonDirectory, "prepare_modules.py"),
            RedirectIoPath = Path.Join(pythonDirectory, "redirect_io.py"),
            RobotPath = Path.Join(pythonDirectory, "robot.py"),
            StandardLibraryPath = Path.Join(pythonDirectory, "Lib")
        });

        _pythonSettingsStorage = A.Fake<ISettingsStorage<PythonSettings>>();
        A.CallTo(() => _pythonSettingsStorage.Value).ReturnsLazily(() => _pythonSettings);
    }

    protected override string CodeDirectory => "Languages/PythonCode";

    protected override ILanguageRunner CreateRunner(IRobotController controller, TextReader input, TextWriter output)
    {
        var runner = new PythonLanguageRunner(
            A.Dummy<ILogger<PythonLanguageRunner>>(),
            _pythonOptions,
            _pythonSettingsStorage);

        runner.Configure(new LanguageRunnerDto
        {
            Language = Language.Python,
            Input = input,
            Output = output,
            Speed = ExecutionSpeed.X20,
            RobotController = controller
        });

        return runner;
    }

    protected override string GetCode(string propertyName)
    {
        var fileName = propertyName.FromPascalCaseToSnakeCase();
        var path = Path.Join(CodeDirectory, $"{fileName}.py");
        return File.ReadAllText(path);
    }

    [Fact]
    public async Task Execute_WhenUseEntryFunctionIsEnabled_CallsEntryFunction()
    {
        _pythonSettings = new PythonSettings
        {
            UseEntryFunction = true,
            EntryFunctionName = "main",
            SupressIo = false
        };

        var (controller, _, _, _) = await ExecuteAsync(GetCode("UseEntryFunctionEnabledCode"));

        A.CallTo(() => controller.MoveUp()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenUseEntryFunctionIsDisabled_DoesNotCallEntryFunction()
    {
        _pythonSettings = new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            SupressIo = false
        };

        var (controller, _, _, _) = await ExecuteAsync(GetCode("UseEntryFunctionDisabledCode"));

        A.CallTo(() => controller.MoveUp()).MustNotHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenSuppressIoIsEnabled_PrintRaisesCodeError()
    {
        _pythonSettings = new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            SupressIo = true
        };

        var (_, _, _, lines) = await ExecuteAsync(GetCode("SuppressIoPrintCode"));

        var error = lines.Select(line => line.CodeError).FirstOrDefault(error => error.CodeErrorType != CodeErrorType.None);
        error.ErrorMessage.Should().Contain("is not supported due to your application settings");
    }

    [Fact]
    public async Task Execute_WhenSuppressIoIsEnabled_InputRaisesCodeError()
    {
        _pythonSettings = new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            SupressIo = true
        };

        var (_, _, _, lines) = await ExecuteAsync(GetCode("SuppressIoInputCode"));

        lines.Should().Contain(line => line.CodeError.CodeErrorType == CodeErrorType.IoBlocked);
    }

    private static void SetEnvironmentVariables()
    {
        var appDirectory = AppContext.BaseDirectory;
        Environment.SetEnvironmentVariable("ZAGGY_APP", appDirectory);
        Environment.SetEnvironmentVariable("ZAGGY_CONFIG", Path.Join(Path.GetTempPath(), "zaggy-config"));
        Environment.SetEnvironmentVariable("ZAGGY_STATE", Path.Join(Path.GetTempPath(), "zaggy-state"));
        Environment.SetEnvironmentVariable("ZAGGY_TEMP", Path.Join(Path.GetTempPath(), "zaggy-temp"));

        Directory.CreateDirectory(Environment.GetEnvironmentVariable("ZAGGY_CONFIG")!);
        Directory.CreateDirectory(Environment.GetEnvironmentVariable("ZAGGY_STATE")!);
        Directory.CreateDirectory(Environment.GetEnvironmentVariable("ZAGGY_TEMP")!);
    }
}
