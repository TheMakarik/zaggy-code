using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ZaggyCode.Avalonia.Options;
using ZaggyCode.Data.Interfaces;
using ZaggyCode.Games;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.Interfaces;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    #region Reactive properties

    [Reactive] private bool _isTerminalVisible = true;
    [Reactive] private bool _isTerminalExists = true;
    [Reactive] private bool _isTerminalMaximized = true;
    [Reactive] private bool _isRunning = false;
    [Reactive] private ExecutionSpeed _executionSpeed;
    [Reactive] private int _textEditorFontSize;

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
    
    #endregion

    #region Services

    private readonly IServiceScopeFactory _factory;
    private readonly IUserStorage _userStorage;
    private readonly FontSizeOptions _fontSizeOptions;
    private readonly ILogger<MainWindowViewModel> _logger;

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
        _textEditorFontSize = userStorage.Current.CodeFontSize;
        _fontSizeOptions = textFontSize.Value;
        MaxFontSize = _fontSizeOptions.MaxFontSize;
        MinFontSize = _fontSizeOptions.MinFontSize;
        _logger = logger;
        
        

        this.WhenAnyPropertyChanged().Subscribe(context =>
        {
            
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
                .Subscribe(onNext =>  userStorage.Current.CodeFontSize = _textEditorFontSize);
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
            try
            {
                var codeObservable = GetCodeToExecute.Handle(Unit.Default);
                await using var scope = _factory.CreateAsyncScope();
                var runner = scope.ServiceProvider.GetRequiredKeyedService<ILanguageRunner>(".lua");
                var code = await codeObservable;
                
                Debug.Assert(TerminalReader is not null);
                Debug.Assert(TerminalWriter is not null);
                
                runner.RedirectIoStreams(TerminalReader, TerminalWriter);
                runner.Execute(code, ExecutionSpeed.X2, new RobotMover(null!, null!, null!));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while running code");
            }
            
        }, TaskCreationOptions.LongRunning);
    }

    [ReactiveCommand]
    private void IncrementEditorFontSize()
    {
        if(TextEditorFontSize < MaxFontSize)
          TextEditorFontSize += 1;
    }
    
    
    [ReactiveCommand]
    private void DecrementEditorFontSize()
    {
        if(TextEditorFontSize  > MinFontSize)
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

    #endregion
    
    #region Static and private methods
 
    
    #endregion
  

   

   
}
