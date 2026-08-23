using System.Reflection;

namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class AboutWindow : Window
{
    private static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly() ?? typeof(AboutWindow).Assembly;

    public AboutWindow()
    {
        InitializeComponent();

        this.GetObservable(Window.WindowDecorationsProperty)
            .Subscribe(decorations => CustomTitleBar.IsVisible = decorations != WindowDecorations.Full);

        PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        AppNameText.Text = EntryAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
            ?? EntryAssembly.GetName().Name;

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
