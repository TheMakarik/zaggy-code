using System.Reflection;

namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class AboutWindow : Window
{
    private static readonly Assembly EntryAssembly = Assembly.GetEntryAssembly() ?? typeof(AboutWindow).Assembly;

    public AboutWindow()
    {
        InitializeComponent();

        CustomTitleBar.IsVisible = WindowDecorations != WindowDecorations.Full;

        AppNameText.Text = EntryAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
            ?? EntryAssembly.GetName().Name;

        VersionText.Text = $"Версия: {EntryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? EntryAssembly.GetName().Version?.ToString()}";

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
