namespace ZaggyCode.Avalonia.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private RowDefinition[]? _savedRowDefinitions = null;
    private readonly Dictionary<object, int> _originalRows = [];
    private bool _isMaximized = false;
    private bool _isTerminalMaximized = false;
    private readonly ScriptCommandLineSession _terminalSession = new ScriptCommandLineSession();
    private LineHighlighter? _currentHighlighter;
    private TextMate.Installation? _textMateInstallation;
    private DispatcherTimer? _fontSizeToastTimer;
    private DispatcherTimer? _fontSizeToastFadeOutTimer;

    public MainWindow()
    {
        InitializeComponent();
        

        HeaderBar.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        MinimizeButton.Click += (_, __) => WindowState = WindowState.Minimized;
        MaximizeButton.Click += (_, __) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        CloseButton.Click += (_, __) => Close();

        PropertyChanged += (_, args) =>
        {
            if (args.Property.Name == nameof(WindowState))
            {
                MaximizeIcon.Kind = WindowState == WindowState.Maximized
                    ? Material.Icons.MaterialIconKind.WindowRestore
                    : Material.Icons.MaterialIconKind.WindowMaximize;
            }
        };

        Terminal.CurrentSession = _terminalSession;

        TextReader reader = _terminalSession.Reader;
        TextWriter writer = _terminalSession.Writer;

        Terminal.PropertyChanged += async void (_, args) =>
        {
            if (args.Property.Name == nameof(Height) && Terminal.Height <= Terminal.MinHeight)
                await ViewModel?.ResizeGridToMax.Handle(Unit.Default)!;
        };

        this.DataContextChanged += (_, __) =>
        {
            Debug.Assert(ViewModel is not null);
            CodeThemeMenu.InvalidateVisual();

            ViewModel.GetCodeToExecute.RegisterHandler(context =>
                Dispatcher.Invoke(() => context.SetOutput(Editor.Text)));

            ViewModel.OpenSettings.RegisterHandler(async context =>
            {
                var settingsWindow = new SettingsWindow { DataContext = context.Input };
                await settingsWindow.ShowDialog(this);
                context.SetOutput(Unit.Default);
            });

            MessageBus.Current.Listen<CodeThemeChangedMessage>()
                .Subscribe(message => ApplyCodeTheme(message.ThemeName));

            MessageBus.Current.Listen<FontSizeToastMessage>()
                .Subscribe(message => ShowFontSizeToast($"Размер шрифта {message.Source} изменён на {message.FontSize}"));

            ViewModel.TerminalReader = reader;
            ViewModel.TerminalWriter = writer;
            ViewModel.Executor = GameMap.Executor;

            ViewModel.ResetMap.RegisterHandler(context =>
            {
                Dispatcher.Invoke(() => GameMap.Reset());
                context.SetOutput(Unit.Default);
            });

            ViewModel.ConcludeRun.RegisterHandler(context =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!GameMap.IsCompleted && !GameMap.IsDead)
                        writer.WriteLine("Цель не достигнута.");

                    GameMap.Reset();
                });

                context.SetOutput(Unit.Default);
            });

            ViewModel.ClearTerminalContent.RegisterHandler(context =>
            {
                Terminal.Clear();
                context.SetOutput(Unit.Default);
            });

            ViewModel.UpdateCodeLine.RegisterHandler(context =>
            {
                var lineNumber = context.Input;

                var wasFoundColor = Application.Current!.TryFindResource("ForegroundDarkColor", out var color);
                Debug.Assert(wasFoundColor);

                this.Dispatcher.Invoke(() =>
                {
                    if (_currentHighlighter is not null)
                    {
                        Editor.TextArea.TextView.BackgroundRenderers.Remove(_currentHighlighter);
                        _currentHighlighter = null;
                    }

                    _currentHighlighter = new LineHighlighter(lineNumber, (Color)color!);
                    Editor.TextArea.TextView.BackgroundRenderers.Add(_currentHighlighter);
                    Editor.TextArea.TextView.Redraw();
                });

                context.SetOutput(Unit.Default);
            });

            ViewModel.StopCodeExecution.RegisterHandler(context =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (_currentHighlighter is not null)
                    {
                        Editor.TextArea.TextView.BackgroundRenderers.Remove(_currentHighlighter);
                        _currentHighlighter = null;
                        Editor.TextArea.TextView.Redraw();
                    }
                });

                context.SetOutput(Unit.Default);
            });

            ViewModel.ResizeGridToMax.RegisterHandler(context =>
            {
                if (!_isMaximized && !_isTerminalMaximized)
                {
                    SaveGridState();

                    MainContentGrid.RowDefinitions.Clear();
                    MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Pixel) });

                    foreach (Control child in MainContentGrid.Children)
                    {
                        if (child is GridSplitter)
                            Grid.SetRow(child, 1);
                        else
                            Grid.SetRow(child, 0);
                    }

                    _isMaximized = true;
                    MainContentGrid.InvalidateMeasure();
                }

                context.SetOutput(Unit.Default);
            });

            ViewModel.BackGridToNormal.RegisterHandler(context =>
            {
                if (_savedRowDefinitions is not null)
                {
                    MainContentGrid.RowDefinitions.Clear();
                    foreach (RowDefinition rowDef in _savedRowDefinitions)
                        MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rowDef.Height.Value, rowDef.Height.GridUnitType) });

                    foreach (Control child in MainContentGrid.Children)
                    {
                        if (_originalRows.TryGetValue(child, out int originalRow))
                        {
                            if (originalRow < MainContentGrid.RowDefinitions.Count)
                                Grid.SetRow(child, originalRow);
                            else
                                Grid.SetRow(child, 0);
                        }

                        if (child is GridSplitter)
                            child.IsVisible = true;
                    }

                    _savedRowDefinitions = null;
                    _originalRows.Clear();
                    _isMaximized = false;
                    _isTerminalMaximized = false;
                    MainContentGrid.InvalidateMeasure();
                }

                context.SetOutput(Unit.Default);
            });
        };
    }

    private void SaveGridState()
    {
        _originalRows.Clear();

        _savedRowDefinitions = new RowDefinition[MainContentGrid.RowDefinitions.Count];
        for (int i = 0; i < MainContentGrid.RowDefinitions.Count; i++)
        {
            _savedRowDefinitions[i] = new RowDefinition
            {
                Height = new GridLength(
                    MainContentGrid.RowDefinitions[i].Height.Value,
                    MainContentGrid.RowDefinitions[i].Height.GridUnitType
                )
            };
        }

        foreach (Control child in MainContentGrid.Children)
        {
            var currentRow = Grid.GetRow(child);
            _originalRows[child] = currentRow;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        var initialThemeName = ViewModel?.CodeTheme ?? "VisualStudioDark";
        if (!Enum.TryParse<ThemeName>(initialThemeName, out var initialTheme))
            initialTheme = ThemeName.VisualStudioDark;

        var registryOptions = new RegistryOptions(initialTheme);
        _textMateInstallation = Editor.InstallTextMate(registryOptions);
        _textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(Language.Python.GetLanguageExtension()).Id));
        ApplyCodeTheme(initialThemeName);

        GameMap.Map = MapView.CreateSampleMap();

        GameMap.Events.LevelCompleted += (_, _) => _terminalSession.Writer.WriteLine("Уровень пройден!");
        GameMap.Events.RobotDead += (_, _) => _terminalSession.Writer.WriteLine("Загги врезался и погиб.");
    }

    private void ApplyCodeTheme(string themeName)
    {
        if (_textMateInstallation is null)
            return;

        if (!Enum.TryParse<ThemeName>(themeName, out var theme))
            theme = ThemeName.VisualStudioDark;

        var registryOptions = new RegistryOptions(theme);
        _textMateInstallation.SetTheme(registryOptions.LoadTheme(theme));
    }
    
    private void ShowFontSizeToast(string message)
    {
        FontSizeToastText.Text = message;
        FontSizeToast.IsVisible = true;
        FontSizeToast.Opacity = 1;

        _fontSizeToastTimer?.Stop();
        _fontSizeToastFadeOutTimer?.Stop();

        var popupOptions = ViewModel?.PopupOptions;
        var displaySeconds = popupOptions?.PopupDisplaySeconds ?? 1.5;
        var fadeOutSeconds = popupOptions?.PopupFadeOutSeconds ?? 0.5;

        _fontSizeToastTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(displaySeconds)
        };

        _fontSizeToastTimer.Tick += (_, _) =>
        {
            _fontSizeToastTimer?.Stop();
            StartFontSizeToastFadeOut(fadeOutSeconds);
        };

        _fontSizeToastTimer.Start();
    }

    private void StartFontSizeToastFadeOut(double fadeOutSeconds)
    {
        const double tickSeconds = 0.05;
        var totalTicks = (int)(fadeOutSeconds / tickSeconds);
        var currentTick = 0;

        _fontSizeToastFadeOutTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(tickSeconds)
        };

        _fontSizeToastFadeOutTimer.Tick += (_, _) =>
        {
            currentTick++;
            var opacity = 1.0 - (double)currentTick / totalTicks;
            FontSizeToast.Opacity = Math.Max(0, opacity);

            if (currentTick < totalTicks)
                return;

            _fontSizeToastFadeOutTimer?.Stop();
            HideFontSizeToast();
        };

        _fontSizeToastFadeOutTimer.Start();
    }

    private void HideFontSizeToast()
    {
        FontSizeToast.Opacity = 0;
        FontSizeToast.IsVisible = false;
    }

    private void CloseFontSizeToastButton_Click(object? sender, RoutedEventArgs e)
    {
        _fontSizeToastTimer?.Stop();
        _fontSizeToastFadeOutTimer?.Stop();
        HideFontSizeToast();
    }

    private void ResizeHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border || border.Tag is not string edgeText)
            return;

        if (!Enum.TryParse<WindowEdge>(edgeText, out var edge))
            return;

        BeginResizeDrag(edge, e);
    }
}

