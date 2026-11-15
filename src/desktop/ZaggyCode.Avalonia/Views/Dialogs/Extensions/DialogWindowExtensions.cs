using Avalonia.Data;
using ZaggyCode.Avalonia.Views.Converters.Common;

namespace ZaggyCode.Avalonia.Views.Dialogs.Extensions;

internal static class DialogWindowExtensions
{
    public static void ApplyThemeDecorations(this Window window)
    {
        var viewSettings = App.Services.GetRequiredService<ISettingsStorage<ViewSettings>>();

        window.Bind(
            Window.WindowDecorationsProperty,
            new Binding($"{nameof(ISettingsStorage<ViewSettings>.Value)}.{nameof(ViewSettings.UseSystemTitleBar)}")
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
            if (IsInteractiveControl(e.Source))
                return;

            window.BeginMoveDrag(e);
        };
    }

    private static bool IsInteractiveControl(object? source)
    {
        if (source is not Visual visual)
            return false;

        return visual.FindAncestorOfType<MenuItem>() is not null
               || visual.FindAncestorOfType<Button>() is not null
               || visual.FindAncestorOfType<ComboBox>() is not null;
    }
}
