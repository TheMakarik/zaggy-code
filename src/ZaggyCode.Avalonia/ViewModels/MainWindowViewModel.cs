namespace ZaggyCode.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Reactive properties

    [Reactive] private bool _isTerminalVisible = true;
    [Reactive] private bool _isTerminalExists = true;
    [Reactive] private bool _isRunning = false;
    [Reactive] private bool _useOsDecoration = false;
    [Reactive] private ExecutionSpeed _executionSpeed;
    [Reactive] private Language _selectedLanguage;
    [Reactive] private int _textEditorFontSize;
    [Reactive] private int _terminalFontSize;

    #endregion

    #region Properties

    public int MaxFontSize { get; init; }
    public int MinFontSize { get; init; }

    public TextReader? TerminalReader { get; set; }
    public TextWriter? TerminalWriter { get; set; }

    #endregion

    #region Interaction

    public readonly Interaction<Unit, Unit> ResizeGridToMax = new();
    public readonly Interaction<Unit, Unit> ClearTerminalContent = new();
    public readonly Interaction<Unit, Unit> BackGridToNormal = new();
    public readonly Interaction<Unit, string> GetCodeToExecute = new();
    public readonly Interaction<int, Unit> UpdateCodeLine = new();
    public readonly Interaction<Unit, Unit> StopCodeExecution = new();

    #endregion

    #region Services

    private readonly IServiceScopeFactory _factory;
    private readonly IUserStorage _userStorage;
    private readonly FontSizeOptions _fontSizeOptions;
    private readonly ILogger<MainWindowViewModel> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Constructors

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IServiceScopeFactory factory,
        IUserStorage userStorage,
        IOptions<FontSizeOptions> textFontSize)
    {
        _factory = factory;
        _userStorage = userStorage;
        _executionSpeed = userStorage.Current.LastSpeed;
        _selectedLanguage = userStorage.Current.LastLanguage;
        _textEditorFontSize = userStorage.Current.CodeFontSize;
        _terminalFontSize = userStorage.Current.TerminalFontSize;
        _fontSizeOptions = textFontSize.Value;
        MaxFontSize = _fontSizeOptions.MaxFontSize;
        MinFontSize = _fontSizeOptions.MinFontSize;
        _logger = logger;

        this.WhenAnyPropertyChanged().Subscribe(context =>
        {
#pragma warning disable AsyncVoidMethod
            this.WhenAnyValue(vm => vm.IsTerminalVisible)
                .Where(isVisible => !isVisible)
                .Subscribe(async void (onNext) => await ResizeGridToMax.Handle(Unit.Default));

            this.WhenAnyValue(vm => vm.IsTerminalVisible)
                .Where(isVisible => isVisible)
                .Subscribe(async void (onNext) => await BackGridToNormal.Handle(Unit.Default));

            this.WhenAnyValue(vm => vm.IsTerminalExists)
                .Where(isVisible => !isVisible)
                .Subscribe(async void (onNext) => await ClearTerminalContent.Handle(Unit.Default));

            this.WhenAnyValue(vm => vm.TextEditorFontSize)
                .Where(size => size != _userStorage.Current.CodeFontSize)
                .Subscribe(onNext => userStorage.Current.CodeFontSize = _textEditorFontSize);

            this.WhenAnyValue(vm => vm.TerminalFontSize)
                .Where(size => size != _userStorage.Current.TerminalFontSize)
                .Subscribe(onNext => userStorage.Current.TerminalFontSize = _terminalFontSize);

            this.WhenAnyValue(vm => vm.ExecutionSpeed)
                .Where(speed => speed != _userStorage.Current.LastSpeed)
                .Subscribe(onNext => userStorage.Current.LastSpeed = _executionSpeed);

            this.WhenAnyValue(vm => vm.SelectedLanguage)
                .Where(language => language != _userStorage.Current.LastLanguage)
                .Subscribe(onNext => userStorage.Current.LastLanguage = _selectedLanguage);
#pragma warning restore AsyncVoidMethod
        });
    }

    #endregion

    #region Reactive commands

    [ReactiveCommand]
    private void CloseTheTerminal()
    {
        IsTerminalVisible = false;
        IsTerminalExists = false;
    }

    [ReactiveCommand]
    private void ExecuteCode()
    {
        _ = Task.Factory.StartNew(async () =>
        {
            await PrepareExecution();
            await RunCode();
            await FinalizeExecution();
        }, TaskCreationOptions.LongRunning);
    }

    [ReactiveCommand]
    private void IncrementEditorFontSize()
    {
        if (TextEditorFontSize < MaxFontSize)
            TextEditorFontSize += 1;
    }

    [ReactiveCommand]
    private void DecrementEditorFontSize()
    {
        if (TextEditorFontSize > MinFontSize)
            TextEditorFontSize -= 1;
    }

    [ReactiveCommand]
    private void ChangeTerminalVisibility()
    {
        IsTerminalExists = true;
        IsTerminalVisible = !IsTerminalVisible;
    }

    [ReactiveCommand]
    private void UpdateFontSize(int fontSize)
    {
        TextEditorFontSize = fontSize;
    }

    [ReactiveCommand]
    private void IncrementTerminalFontSize()
    {
        if (TerminalFontSize < MaxFontSize)
            TerminalFontSize += 1;
    }

    [ReactiveCommand]
    private void DecrementTerminalFontSize()
    {
        if (TerminalFontSize > MinFontSize)
            TerminalFontSize -= 1;
    }

    [ReactiveCommand]
    private void UpdateTerminalFontSize(int fontSize)
    {
        TerminalFontSize = fontSize;
    }

    [ReactiveCommand]
    private void ChangeExecutionSpeed(ExecutionSpeed speed)
    {
        ExecutionSpeed = speed;
    }

    [ReactiveCommand]
    private void ChangeLanguage(Language language)
    {
        SelectedLanguage = language;
    }

    #endregion

    #region Private methods

    private async Task PrepareExecution()
    {
        lock (this)
        {
            if (_isRunning)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
            else
            {
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        await StopCodeExecution.Handle(Unit.Default);
    }

    private async Task RunCode()
    {
        try
        {
            var code = await GetCodeToExecute.Handle(Unit.Default);
            
            await using var scope = _factory.CreateAsyncScope();
            var runner = scope.ServiceProvider.GetRequiredKeyedService<ILanguageRunner>(SelectedLanguage.GetLanguageExtension());

            Debug.Assert(TerminalReader is not null);
            Debug.Assert(TerminalWriter is not null);

            runner.DebugLineUpdated += OnDebugLineUpdated;
            runner.CodeErrorOccurred += OnCodeErrorOccurred;
                
            Debug.Assert(_cancellationTokenSource is not null);;

            runner
                .RedirectIo(TerminalReader, TerminalWriter)
                .SetExecutor(null!)
                .SetSpeed(ExecutionSpeed)
                .Execute(code, _cancellationTokenSource.Token);
        }
        catch (LuaIncorrectlyWroteNameException e)
        {
            await TerminalWriter!.WriteLineAsync(
                $"{e.Actual} не существуект, возможно вы имели ввиде {e.Suggestion}? \nДля отключение автоматического остановления программы при nil:  TABLE_CONTENT_CHECKER = false");
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Code execution was cancelled");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while running code");
        }
    }

    private async Task FinalizeExecution()
    {
        lock (this)
        {
            _isRunning = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
        
        await StopCodeExecution.Handle(Unit.Default);
    }
#pragma warning disable AsyncVoidEventHandlerMethod
    private async void OnDebugLineUpdated(object? sender, DebugLineUpdatedEventArgs args)
#pragma warning restore AsyncVoidEventHandlerMethod
    {
        await UpdateCodeLine.Handle(args.LineNumber);
    }

    private void OnCodeErrorOccurred(object? sender, CodeErrorOccurredEventArgs args)
    {
        _logger.LogError("Code error: {Text}", args.Text);
    }

    #endregion
}