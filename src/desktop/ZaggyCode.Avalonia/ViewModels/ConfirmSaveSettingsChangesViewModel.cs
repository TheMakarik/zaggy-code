namespace ZaggyCode.Avalonia.ViewModels;

public sealed partial class ConfirmSaveSettingsChangesViewModel : ViewModelBase
{
    public Interaction<bool, Unit> CloseInteraction { get; } = new();

    [ReactiveCommand]
    private async Task Confirm()
    {
        await CloseInteraction.Handle(true);
    }

    [ReactiveCommand]
    private async Task Decline()
    {
        await CloseInteraction.Handle(false);
    }
}
