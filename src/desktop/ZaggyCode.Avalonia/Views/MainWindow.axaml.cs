namespace ZaggyCode.Avalonia.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private RowDefinition[]? _savedRowDefinitions = null;
    private readonly Dictionary<object, int> _originalRows = [];
    private bool _isMaximized = false;
    private readonly ScriptCommandLineSession _terminalSession = new ScriptCommandLineSession();
    private RobotGameEditorProxy? _editorProxy;
    private RobotGameTerminalProxy? _terminalProxy;
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

        MessageBus.Current.Listen<AppThemeChangedMessage>()
            .Subscribe(message => Dispatcher.UIThread.Post(() => _ = ApplyAppThemeAsync(message.ThemeName)));

        var backspaceGesture = new KeyGesture(Key.Back);
        DeleteSelectionMenuItem.InputGesture = backspaceGesture;
        CompactDeleteSelectionMenuItem.InputGesture = backspaceGesture;
        EditorDeleteContextMenuItem.InputGesture = backspaceGesture;

        Terminal.PropertyChanged += (_, args) =>
        {
            if (args.Property.Name == nameof(Height) && Terminal.Height <= Terminal.MinHeight)
                MaximizeTerminalArea();
        };

        this.DataContextChanged += (_, __) => OnDataContextChanged();
    }

    private void OnDataContextChanged()
    {
        Debug.Assert(ViewModel is not null);

        var icon = ViewModel.ZaggyAssets.Value.IconPath;
        this.Icon = new WindowIcon(new Bitmap(icon));

        CodeThemeMenu.InvalidateVisual();

        AttachGameProxies();
        RegisterInteractionHandlers();
    }

    private void AttachGameProxies()
    {
        _editorProxy = App.Services.GetRequiredService<RobotGameEditorProxy>();
        _editorProxy.Attach(Editor);

        _terminalProxy = App.Services.GetRequiredService<RobotGameTerminalProxy>();
        _terminalProxy.Attach(_terminalSession);
        
    }

    private async Task ApplyAppThemeAsync(string themeName)
    {
        if (App.Services.GetRequiredService<IThemeApplier>() is not { } applier)
            return;

        try
        {
            await applier.ApplyThemeAsync(themeName);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Failed to apply app theme {Theme}", themeName);
        }
    }

    private void RegisterInteractionHandlers()
    {
        ViewModel.GetCodeToExecute.RegisterHandler(context =>
            Dispatcher.Invoke(() => context.SetOutput(Editor.Text)));

        ViewModel.GetSelectedCode.RegisterHandler(context =>
            context.SetOutput(Editor.SelectedText));

        ViewModel.GetTerminalStreams.RegisterHandler(context =>
            context.SetOutput((_terminalProxy!.Input, _terminalProxy!.Output)));

        ViewModel.OpenSettings.RegisterHandler(async context =>
        {
            var settingsWindow = new SettingsWindow { DataContext = context.Input };
            await settingsWindow.ShowDialog(this);
            context.SetOutput(Unit.Default);
        });

        ViewModel.OpenAbout.RegisterHandler(async context =>
        {
            var aboutWindow = new AboutWindow
            {
                WindowDecorations = ViewModel.UseOsDecoration ? WindowDecorations.Full : WindowDecorations.BorderOnly
            };
            await aboutWindow.ShowDialog(this);
            context.SetOutput(Unit.Default);
        });

        ViewModel.NotImplementedOccurred.RegisterHandler(async context =>
        {
            var notImplementedWindow = new NotImplementedWindow
            {
                WindowDecorations = ViewModel.UseOsDecoration ? WindowDecorations.Full : WindowDecorations.BorderOnly
            };
            await notImplementedWindow.ShowDialog(this);
            context.SetOutput(Unit.Default);
        });

        ViewModel.ShowToast.RegisterHandler(context =>
        {
            ShowFontSizeToast(context.Input);
            context.SetOutput(Unit.Default);
        });

        ViewModel.ApplyCodeTheme.RegisterHandler(context =>
        {
            ApplyCodeTheme(context.Input);
            context.SetOutput(Unit.Default);
        });

        ViewModel.ResetMap.RegisterHandler(context =>
        {
            Dispatcher.Invoke(() => GameMapView.Reset());
            context.SetOutput(Unit.Default);
        });

        ViewModel.ConcludeRun.RegisterHandler(context =>
        {
            Dispatcher.Invoke(() => GameMapView.Reset());
            context.SetOutput(Unit.Default);
        });

        ViewModel.ClearTerminalContent.RegisterHandler(context =>
        {
            Terminal.Clear();
            context.SetOutput(Unit.Default);
        });

        ViewModel.UpdateCodeLine.RegisterHandler(context =>
        {
            Debug.Assert(_editorProxy is not null);
            _editorProxy!.SetCurrentExecutionLine(context.Input);
            context.SetOutput(Unit.Default);
        });

        ViewModel.StopCodeExecution.RegisterHandler(context =>
        {
            Debug.Assert(_editorProxy is not null);
            _editorProxy!.SetCurrentExecutionLine(0);
            context.SetOutput(Unit.Default);
        });

        ViewModel.ResizeGridToMax.RegisterHandler(context =>
        {
            MaximizeTerminalArea();
            context.SetOutput(Unit.Default);
        });

        ViewModel.BackGridToNormal.RegisterHandler(context =>
        {
            RestoreGridArea();
            context.SetOutput(Unit.Default);
        });
    }

    private void EditorCut_Click(object? sender, RoutedEventArgs e) => Editor.Cut();

    private void EditorCopy_Click(object? sender, RoutedEventArgs e) => Editor.Copy();

    private void EditorPaste_Click(object? sender, RoutedEventArgs e) => Editor.Paste();

    private void EditorDeleteSelection_Click(object? sender, RoutedEventArgs e) => Editor.SelectedText = string.Empty;

    private void SaveGridState()
    {
        _originalRows.Clear();

        _savedRowDefinitions = new RowDefinition[MainContentGrid.RowDefinitions.Count];
        for (var i = 0; i < MainContentGrid.RowDefinitions.Count; i++)
        {
            _savedRowDefinitions[i] = new RowDefinition
            {
                Height = new GridLength(
                    MainContentGrid.RowDefinitions[i].Height.Value,
                    MainContentGrid.RowDefinitions[i].Height.GridUnitType
                )
            };
        }

        foreach (var child in MainContentGrid.Children)
        {
            var currentRow = Grid.GetRow(child);
            _originalRows[child] = currentRow;
        }
    }

    private void MaximizeTerminalArea()
    {
        if (_isMaximized)
            return;

        SaveGridState();

        MainContentGrid.RowDefinitions.Clear();
        MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Pixel) });

        foreach (var child in MainContentGrid.Children)
        {
            if (child is GridSplitter)
                Grid.SetRow(child, 1);
            else
                Grid.SetRow(child, 0);
        }

        _isMaximized = true;
        MainContentGrid.InvalidateMeasure();
    }

    private void RestoreGridArea()
    {
        if (_savedRowDefinitions is null)
            return;

        MainContentGrid.RowDefinitions.Clear();
        foreach (var rowDefinition in _savedRowDefinitions)
            MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rowDefinition.Height.Value, rowDefinition.Height.GridUnitType) });

        foreach (var child in MainContentGrid.Children)
        {
            if (_originalRows.TryGetValue(child, out var originalRow))
                Grid.SetRow(child, originalRow < MainContentGrid.RowDefinitions.Count ? originalRow : 0);

            if (child is GridSplitter)
                child.IsVisible = true;
        }

        _savedRowDefinitions = null;
        _originalRows.Clear();
        _isMaximized = false;
        MainContentGrid.InvalidateMeasure();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ViewModel?.WhenAnyValue(vm => vm.EnableCodeHighlighting)
            .Subscribe(ApplyCodeHighlighting);

        _ = ViewModel.InitializeGameEngineAsync();
    }

    private void ApplyCodeHighlighting(bool isEnabled)
    {
        if (isEnabled)
        {
            InstallCodeHighlighting();
            return;
        }

        _textMateInstallation?.Dispose();
        _textMateInstallation = null;
    }

    private void InstallCodeHighlighting()
    {
        if (_textMateInstallation is not null)
            return;

        var themeName = ViewModel?.CodeTheme ?? "VisualStudioDark";
        if (!Enum.TryParse<ThemeName>(themeName, out var theme))
            theme = ThemeName.VisualStudioDark;

        var registryOptions = new RegistryOptions(theme);
        _textMateInstallation = Editor.InstallTextMate(registryOptions);
        _textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(Language.Python.GetLanguageExtension()).Id));
        _textMateInstallation.SetTheme(registryOptions.LoadTheme(theme));
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
