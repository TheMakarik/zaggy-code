using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData.Binding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ZaggyCode.Data.Interfaces;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Avalonia.ViewModels;

[SingletonService]
public partial class MainWindowViewModel : ViewModelBase
{
    
    #region Reactive properties

    [Reactive] private bool _isTerminalVisible = true;
    [Reactive] private bool _isTerminalExists = true;
    [Reactive] private bool _isTerminalMaximized = true;
    [Reactive] private bool _isRunning = false;
    [Reactive] private ExecutionSpeed _executionSpeed;

    #endregion    
    
    #region Interaction

    public readonly Interaction<Unit, Unit> ResizeGridToMax = new();
    public readonly Interaction<Unit, Unit> ClearTerminalContent = new();
    public readonly Interaction<Unit, Unit> BackGridToNormal = new();
    public readonly Interaction<Unit, Unit> MaximizeTerminal = new();
    
    #endregion

    #region Services

    private readonly IServiceScopeFactory _factory;
    private readonly IUserStorage _userStorage;

    #endregion

    #region Constructors

    public MainWindowViewModel(IServiceScopeFactory factory, IUserStorage userStorage)
    {
        _factory = factory;
        _userStorage = userStorage;

        _executionSpeed = userStorage.Current.LastSpeed;


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
    private async Task MaximizeOrNormalizeTerminal()
    {
        if(IsTerminalMaximized)
            await BackGridToNormal.Handle(Unit.Default);
        else
        
            await MaximizeTerminal.Handle(Unit.Default);

        IsTerminalMaximized = !IsTerminalMaximized;

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
        _userStorage.Current.CodeFontSize = fontSize;
    }

    #endregion
    
    #region Static and private methods
 
    
    #endregion
  

   

   
}
