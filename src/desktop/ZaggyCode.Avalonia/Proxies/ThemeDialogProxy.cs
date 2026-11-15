namespace ZaggyCode.Avalonia.Proxies;

//#:NO_AI
public sealed class ThemeDialogProxy : IThemeDialogProxy
{
    public async Task ShowThemeNotFoundDialogAsync(string missingThemeName)
    {
        var owner = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
        if (owner is null)
            return;

        var dialog = new ThemeNotFoundWindow(missingThemeName);
        await dialog.ShowDialog(owner);
    }
}
