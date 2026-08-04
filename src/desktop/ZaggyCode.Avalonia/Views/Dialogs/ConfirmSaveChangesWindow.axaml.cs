using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ConfirmSaveChangesWindow : Window
{
    public ConfirmSaveChangesWindow()
    {
        InitializeComponent();
        CustomTitleBar.IsVisible = WindowDecorations != WindowDecorations.Full;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
