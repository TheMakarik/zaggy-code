using Avalonia.VisualTree;

namespace ZaggyCode.Avalonia.Views.UserControls;

public partial class ThemesSettingsView : ReactiveUserControl<AppearanceSettingsViewModel>
{
    public ThemesSettingsView()
    {
        InitializeComponent();
    }

    private void OnThemeCardPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Button button && button.GetVisualDescendants().OfType<Popup>().FirstOrDefault() is { } popup)
            popup.IsOpen = true;
    }

    private void OnThemeCardPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Button button && button.GetVisualDescendants().OfType<Popup>().FirstOrDefault() is { } popup)
            popup.IsOpen = false;
    }
}
