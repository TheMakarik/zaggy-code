namespace ZaggyCode.Tests.Languages;

public class PythonLanguageRunnerTests : LanguageRunnerTests
{
    private readonly IServiceProvider _serviceProvider;

    protected string UseEntryFunctionEnabledCode => GetCode(nameof(UseEntryFunctionEnabledCode));
    protected string UseEntryFunctionDisabledCode => GetCode(nameof(UseEntryFunctionDisabledCode));
    protected string SuppressIoPrintCode => GetCode(nameof(SuppressIoPrintCode));
    protected string SuppressIoInputCode => GetCode(nameof(SuppressIoInputCode));

    public PythonLanguageRunnerTests()
    {
        SetEnvironmentVariables();

        var configuration = new ConfigurationManager()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var fakeSpeedOptions = A.Fake<IOptions<SpeedMillisecondsOptions>>();

        A.CallTo(() => fakeSpeedOptions.Value).Returns(new SpeedMillisecondsOptions()
        {
            SleepChunk = 1, //Будет ошибка деления на ноль при нуле, оставить 1.
            X1 = 0,
            X2 = 0,
            X5 = 0,
            X10 = 3,
            X20 = 0
        });

        var services = new ServiceCollection()
            .AddSingleton<ILanguageSleepHelper, LanguageSleepHelper>()
            .AddSingleton<IConfiguration>(configuration)
            .Configure<PythonScriptsOptions>(configuration.GetSection(nameof(PythonScriptsOptions)))
            .AddSingleton<IOptions<SpeedMillisecondsOptions>>(fakeSpeedOptions)
            .AddSingleton<IPythonScopeFactory>(provider => new PythonScopeFactory(
                A.Dummy<ILogger<PythonScopeFactory>>(),
                provider.GetRequiredService<IOptions<PythonScriptsOptions>>()))
            .AddSingleton<IObservableStorage<PythonSettings>>(CreatePythonSettingsStorage)
            .AddSingleton<IObservableStorage<UserData>>(CreateUserStorage)
            .AddSingleton<ILogger<PythonLanguageRunner>>(A.Dummy<ILogger<PythonLanguageRunner>>())
            .AddSingleton<PythonLanguageRunner>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override ILanguageRunner SystemUnderTests => _serviceProvider.GetRequiredService<PythonLanguageRunner>();
    protected override string CodeDirectory => "Languages/PythonCode";
    
    protected override string GetCode(string propertyName)
    {
        var fileName = propertyName.FromPascalCaseToSnakeCase();
        var path = Path.Join(CodeDirectory, $"{fileName}.py");
        return File.ReadAllText(path);
    }

    [Fact]
    public async Task Execute_WhenUseEntryFunctionIsEnabled_CallsEntryFunction()
    {
        // Arrange
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IObservableStorage<PythonSettings>>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = true,
            EntryFunctionName = "main",
            SupressIo = false
        });

        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.Execute(UseEntryFunctionEnabledCode, CancellationToken.None);

        // Assert
        A.CallTo(() => executor.MoveUp()).MustHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenUseEntryFunctionIsDisabled_DoesNotCallEntryFunction()
    {
        // Arrange
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IObservableStorage<PythonSettings>>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            SupressIo = false
        });

        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);

        // Act
        await SystemUnderTests.Execute(UseEntryFunctionDisabledCode, CancellationToken.None);

        // Assert
        A.CallTo(() => executor.MoveUp()).MustNotHaveHappened();
    }

    [Fact]
    public async Task Execute_WhenSuppressIoIsEnabled_PrintRaisesCodeErrorOccurred()
    {
        // Arrange
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IObservableStorage<PythonSettings>>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            SupressIo = true
        });

        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);
        CodeErrorOccurredEventArgs? capturedArgs = null;
        SystemUnderTests.CodeErrorOccurred += (_, args) => capturedArgs = args;

        // Act
        await SystemUnderTests.Execute(SuppressIoPrintCode, CancellationToken.None);

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Text.Should().Contain("is not supported due to your application settings");
    }

    [Fact]
    public async Task Execute_WhenSuppressIoIsEnabled_InputRaisesCodeErrorOccurred()
    {
        // Arrange
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IObservableStorage<PythonSettings>>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            SupressIo = true
        });

        var executor = A.Fake<IRobotExecutor>();
        var (input, output) = ConfigureRunner(executor);
        CodeErrorOccurredEventArgs? capturedArgs = null;
        SystemUnderTests.CodeErrorOccurred += (_, args) => capturedArgs = args;

        // Act
        await SystemUnderTests.Execute(SuppressIoInputCode, CancellationToken.None);

        // Assert
        capturedArgs.Should().NotBeNull();
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

    private static IObservableStorage<PythonSettings> CreatePythonSettingsStorage(IServiceProvider provider)
    {
        var storage = A.Fake<IObservableStorage<PythonSettings>>();
        A.CallTo(() => storage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            SupressIo = false
        });
        return storage;
    }

    private static IObservableStorage<UserData> CreateUserStorage(IServiceProvider provider)
    {
        var storage = A.Fake<IObservableStorage<UserData>>();
        A.CallTo(() => storage.Current).Returns(new UserData
        {
            EnableCodeHighlighting = true,
            ShowCodeLineNumbers = true,
            CodeFontSize = 14,
            CodeTheme = "Light",
            LastLanguage = Language.Python,
            LastGamePath = null,
            LastSpeed = ExecutionSpeed.X1,
            TerminalFontSize = 18,
            UseSystemTitleBar = false,
            ShowSidebar = true
        });
        return storage;
    }
}
