namespace ZaggyCode.Avalonia.Views.Dialogs;

public partial class ConfirmSaveChangesWindow : Window
{
    public ConfirmSaveChangesWindow()
    {
        InitializeComponent();
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
