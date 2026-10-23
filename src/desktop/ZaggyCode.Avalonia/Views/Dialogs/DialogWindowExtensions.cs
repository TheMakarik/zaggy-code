using Avalonia.VisualTree;

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

    public static void ApplyZaggyIcon(this Window window)
    {
        var iconPath = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.IconPath;
        window.Icon = new WindowIcon(iconPath);
    }

    public static void EnableWindowDrag(this Window window)
    {
        window.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
                return;

            // Клики по меню, кнопкам и выпадающим спискам не должны превращаться в перетаскивание окна.
            if (e.Source is Visual source &&
                (source.FindAncestorOfType<MenuItem>() is not null ||
                 source.FindAncestorOfType<Button>() is not null ||
                 source.FindAncestorOfType<ComboBox>() is not null))
                return;

            window.BeginMoveDrag(e);
        };
    }
}
