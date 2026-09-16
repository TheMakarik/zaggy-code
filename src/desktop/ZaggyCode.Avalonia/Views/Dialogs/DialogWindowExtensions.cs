namespace ZaggyCode.Avalonia.Views.Dialogs;

internal static class DialogWindowExtensions
{
    public static void ApplyThemeDecorations(this Window window)
    {
        var useSystemTitleBar = App.Services.GetRequiredService<IObservableStorage<UserData>>().Current.UseSystemTitleBar;

        window.WindowDecorations = useSystemTitleBar
            ? global::Avalonia.Controls.WindowDecorations.Full
            : global::Avalonia.Controls.WindowDecorations.BorderOnly;
    }
}
