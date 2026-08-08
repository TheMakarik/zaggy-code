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

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<PythonScriptsOptions>(configuration.GetSection(nameof(PythonScriptsOptions)));
        services.Configure<SpeedMillisecondsOptions>(configuration.GetSection(nameof(SpeedMillisecondsOptions)));
        services.AddSingleton<IPythonScopeFactory>(provider => new PythonScopeFactory(A.Dummy<ILogger<PythonScopeFactory>>(), provider.GetRequiredService<IOptions<PythonScriptsOptions>>()));
        services.AddSingleton<IPythonSettingsStorage>(CreatePythonSettingsStorage);
        services.AddSingleton<IUserStorage>(CreateUserStorage);
        services.AddSingleton<ILogger<PythonLanguageRunner>>(A.Dummy<ILogger<PythonLanguageRunner>>());
        services.AddSingleton<PythonLanguageRunner>();

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
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IPythonSettingsStorage>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = true,
            EntryFunctionName = "main",
            DetailedExceptions = true,
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
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IPythonSettingsStorage>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            DetailedExceptions = true,
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
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IPythonSettingsStorage>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            DetailedExceptions = true,
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
        var pythonSettingsStorage = _serviceProvider.GetRequiredService<IPythonSettingsStorage>();
        A.CallTo(() => pythonSettingsStorage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            DetailedExceptions = true,
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
        capturedArgs!.Text.Should().Contain("is not supported due to your application settings");
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

    private static IPythonSettingsStorage CreatePythonSettingsStorage(IServiceProvider provider)
    {
        var storage = A.Fake<IPythonSettingsStorage>();
        A.CallTo(() => storage.Current).Returns(new PythonSettings
        {
            UseEntryFunction = false,
            EntryFunctionName = "main",
            DetailedExceptions = true,
            SupressIo = false
        });
        return storage;
    }

    private static IUserStorage CreateUserStorage(IServiceProvider provider)
    {
        var storage = A.Fake<IUserStorage>();
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
