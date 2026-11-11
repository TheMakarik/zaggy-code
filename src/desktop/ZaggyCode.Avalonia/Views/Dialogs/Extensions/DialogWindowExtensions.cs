using Avalonia.Data;
using ZaggyCode.Avalonia.Views.Converters;

namespace ZaggyCode.Avalonia.Views.Dialogs.Extensions;

internal static class DialogWindowExtensions
{
    public static void ApplyThemeDecorations(this Window window)
    {
        var viewSettings = App.Services.GetRequiredService<IObservableStorage<ViewSettings>>();

        window.Bind(
            Window.WindowDecorationsProperty,
            new Binding($"{nameof(IObservableStorage<ViewSettings>.Current)}.{nameof(ViewSettings.UseSystemTitleBar)}")
            {
                Source = viewSettings,
                Converter = new BoolToWindowDecorationsConverter()
            });
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
