using System.Reactive;
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
    [Reactive] private bool _isRunning = false;
    [Reactive] private ExecutionSpeed _executionSpeed;

    #endregion    
    
    #region Interaction
    
    public Interaction<Unit, Unit> ClearTerminalContent = new();
    
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
        
    }

    #endregion

    #region Reactive commands

    [ReactiveCommand]
    private void CloseTheTerminal()
    {
        HideTheTerminal();
        _isTerminalExists = false;
    }
    
    [ReactiveCommand]
    private void HideTheTerminal()
    {
        _isTerminalVisible = false;
    }

    #endregion
    
    #region Static and private methods
 
    
    #endregion
  

   

   
}