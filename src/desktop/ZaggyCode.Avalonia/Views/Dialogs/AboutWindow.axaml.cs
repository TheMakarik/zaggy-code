using System.Reflection;

namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class AboutWindow : Window
{
    private static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly() ?? typeof(AboutWindow).Assembly;

    public AboutWindow()
    {
        InitializeComponent();

        // WindowDecorations из object initializer применяется после конструктора,
        // поэтому следим за изменением, а не проверяем разово.
        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        // Перетаскивание за любую область окна: клики по кнопкам не доходят сюда,
        // потому что Button помечает PointerPressed как обработанный.
        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        AppNameText.Text = EntryAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
            ?? EntryAssembly.GetName().Name;

        // InformationalVersion содержит хеш коммита после '+' — берём только версию из csproj.
        var informationalVersion = EntryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var version = informationalVersion?.Split('+').First()
            ?? EntryAssembly.GetName().Version?.ToString();

        VersionText.Text = $"Версия: {version}";

        DescriptionText.Text = EntryAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        DevelopersText.Text = EntryAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
    }

    private void LoadLogo(object? sender, RoutedEventArgs e)
    {
        if (sender is not SvgFromContent control)
            return;

        control.Path = App.Services.GetRequiredService<IOptions<ZaggyAssetsOptions>>().Value.LogoPath;
    }
}
