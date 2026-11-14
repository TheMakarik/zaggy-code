using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Material.Icons.Avalonia;

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

            // Биндинг команды из ContextMenu не подхватывает DataContext корневой панели,
            // из-за чего пункт меню остаётся задизейбленым — выставляем команду вручную.
            if (this.FindControl<MenuItem>("CreateThemeMenuItem") is { } createThemeMenuItem)
                createThemeMenuItem.Command = viewModel.AddNewThemeCommand;

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

            viewModel.ShowThemeFileInteraction.RegisterHandler(async context =>
            {
                if (TopLevel.GetTopLevel(this)?.Launcher is not { } launcher)
                {
                    context.SetOutput(Unit.Default);
                    return;
                }

                // Показываем папку с темой: выделение файла средствами лаунчера не поддерживается.
                var directory = Path.GetDirectoryName(context.Input);
                if (directory is not null)
                    await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(directory));

                context.SetOutput(Unit.Default);
            });

            viewModel.CopyThemeFileInteraction.RegisterHandler(async context =>
            {
                if (TopLevel.GetTopLevel(this) is not { } topLevel)
                {
                    context.SetOutput(Unit.Default);
                    return;
                }

                if (await topLevel.StorageProvider.TryGetFileFromPathAsync(context.Input) is { } file && topLevel.Clipboard is { } clipboard)
                    await clipboard.SetFileAsync(file);

                context.SetOutput(Unit.Default);
            });
        };

        InitializeSearchBorderHighlight();
    }

    // Цвет иконки поиска синхронизирован с бордером поля: обычный, при ховере и при
    // фокусе — подсвеченный. Меняем напрямую в событиях TextBox, биндинги не подхватываются.
    private void InitializeSearchBorderHighlight()
    {
        if (this.FindControl<TextBox>("ThemeSearchBox") is not { } searchBox)
            return;

        var icon = this.FindControl<MaterialIcon>("ThemeSearchIcon");
        ApplySearchBrush(searchBox, icon, "BorderBrush");

        searchBox.PointerEntered += (_, _) => ApplySearchBrush(searchBox, icon, "BorderHighlightBrush");
        searchBox.PointerExited += (_, _) =>
        {
            if (!searchBox.IsFocused)
                ApplySearchBrush(searchBox, icon, "BorderBrush");
        };
        searchBox.GotFocus += (_, _) => ApplySearchBrush(searchBox, icon, "BorderHighlightBrush");
        searchBox.LostFocus += (_, _) =>
        {
            if (!searchBox.IsPointerOver)
                ApplySearchBrush(searchBox, icon, "BorderBrush");
        };
    }

    private static void ApplySearchBrush(TextBox searchBox, MaterialIcon? icon, string resourceKey)
    {
        if (searchBox.TryFindResource(resourceKey, out var resource) && resource is IBrush brush)
        {
            searchBox.BorderBrush = brush;
            if (icon is not null)
                icon.Foreground = brush;
        }
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
