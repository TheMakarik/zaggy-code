using Avalonia.VisualTree;
using ZaggyCode.Avalonia.Views.Dialogs;

namespace ZaggyCode.Avalonia.Views.UserControls;

public partial class ThemesSettingsView : ReactiveUserControl<AppearanceSettingsViewModel>
{
    public ThemesSettingsView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is not AppearanceSettingsViewModel viewModel)
                return;

            viewModel.OpenThemeEditor.RegisterHandler(async context =>
            {
                if (TopLevel.GetTopLevel(this) is not Window owner)
                {
                    context.SetOutput(Unit.Default);
                    return;
                }

                var window = new ThemeEditorWindow
                {
                    WindowDecorations = owner.WindowDecorations,
                    DataContext = context.Input
                };

                await window.ShowDialog(owner);
                context.SetOutput(Unit.Default);
            });

            viewModel.OpenAddTheme.RegisterHandler(async context =>
            {
                if (TopLevel.GetTopLevel(this) is not Window owner)
                {
                    context.SetOutput(null);
                    return;
                }

                var window = new AddThemeWindow
                {
                    WindowDecorations = owner.WindowDecorations,
                    DataContext = context.Input
                };

                context.SetOutput(await window.ShowDialog<string?>(owner));
            });
        };
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
