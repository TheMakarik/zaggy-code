using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.TextMate;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using TextMateSharp.Grammars;
using VirtualTerminal.Session;
using ZaggyCode.Avalonia.ViewModels;

namespace ZaggyCode.Avalonia.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private RowDefinition[]? _savedRowDefinitions = null;
    private Dictionary<object, int> _originalRows = [];
    private bool _isMaximized = false;
    private bool _isTerminalMaximized = false;
    private ScriptCommandLineSession _terminalSession = new ScriptCommandLineSession();

    public MainWindow()
    {
        InitializeComponent();

        Editor.TextArea.KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.V, KeyModifiers.Control),
            Command = ReactiveCommand.Create(() =>
            {
                IAsyncDataTransfer? clipboardData = Clipboard?.TryGetDataAsync().Result;
                var textData = clipboardData?.TryGetTextAsync().Result;
                if (textData != null)
                {
                    Editor.Text = textData;
                }
            })
        });

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

            ViewModel.GetCodeToExecute.RegisterHandler(context =>
                Dispatcher.Invoke(() => context.SetOutput(Editor.Text)));

            ViewModel.TerminalReader = reader;
            ViewModel.TerminalWriter = writer;
            ViewModel.ClearTerminalContent.RegisterHandler(context =>
            {
                Terminal.Clear();
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
                if (_savedRowDefinitions != null)
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


        RegistryOptions registryOptions = new RegistryOptions(ThemeName.VisualStudioDark);
        TextMate.Installation textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".lua").Id));

    }
}