namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class ConfirmThemeSaveViewModel(string themeName) : ViewModelBase
{
    public string ThemeName { get; } = themeName;

    public Interaction<bool?, Unit> CloseInteraction { get; } = new();

    [ReactiveCommand]
    private async Task Confirm()
        => await CloseInteraction.Handle(true);

    [ReactiveCommand]
    private async Task Decline()
        => await CloseInteraction.Handle(false);

    [ReactiveCommand]
    private async Task Cancel()
        => await CloseInteraction.Handle(null);
}
