using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.TextMate;
using ReactiveUI.Avalonia;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using TextMateSharp.Grammars;
using ZaggyCode.Avalonia.ViewModels;
using ZaggyCode.Avalonia.Views.TerminalEngine.Session;

namespace ZaggyCode.Avalonia.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private RowDefinition[]? _savedRowDefinitions = null;
    private Dictionary<object, int> _originalRows = [];
    private bool _isMaximized = false;
    private bool _isTerminalMaximized = false;
    private ScriptCommandLineSession _terminalSession = new ScriptCommandLineSession();

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            _terminalSession.Writer.Write("Твоё имя : ");
            string? line = _terminalSession.Reader.ReadLine();
            _terminalSession.Writer.WriteLine(line);
        });
    }

    public MainWindow()
    {
        InitializeComponent();
        Terminal.CurrentSession = _terminalSession;

        // на.
        TextReader reader = _terminalSession.Reader;
        TextWriter writer = _terminalSession.Writer;

        this.DataContextChanged += (_, __) =>
        {
            ViewModel!.ClearTerminalContent.RegisterHandler(context =>
            {

                /*
                 Кое кто забыл добавить поддержку очистки терминала 

                RikitavTimur:
                Нахуй иди
                 */

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

                context.SetOutput(Unit.Default);
            });

            ViewModel.BackGridToNormal.RegisterHandler(context =>
            {
                if (_savedRowDefinitions != null)
                {
                    MainContentGrid.RowDefinitions.Clear();
                    foreach (var rowDef in _savedRowDefinitions)
                        MainContentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rowDef.Height.Value, rowDef.Height.GridUnitType) });

                    foreach (var child in MainContentGrid.Children)
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

        foreach (var child in MainContentGrid.Children)
        {
            var currentRow = Grid.GetRow(child);
            _originalRows[child] = currentRow;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var registryOptions = new RegistryOptions(ThemeName.VisualStudioLight);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".lua").Id));
    }
}