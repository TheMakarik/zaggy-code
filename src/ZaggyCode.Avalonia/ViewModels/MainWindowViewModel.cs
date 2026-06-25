using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.SourceGenerators;
using ZaggyCode.Data.Interfaces;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Avalonia.ViewModels;

[SingletonService]
public partial class MainWindowViewModel : ViewModelBase
{

    [Reactive] private bool _isTerminalVisible = true;
    [Reactive] private bool _isTerminalExists = true;
    [Reactive] private bool _isRunning = false;
    [Reactive] private ExecutionSpeed _executionSpeed;
    
    private readonly IServiceScopeFactory _factory;
    private readonly IUserStorage _userStorage;

    public MainWindowViewModel(IServiceScopeFactory factory, IUserStorage userStorage)
    {
        _factory = factory;
        _userStorage = userStorage;

        _executionSpeed = userStorage.Current.LastSpeed;
        
    }
}