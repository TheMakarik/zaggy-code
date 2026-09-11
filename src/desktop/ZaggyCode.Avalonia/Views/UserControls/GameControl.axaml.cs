namespace ZaggyCode.Avalonia.Views.UserControls;

public partial class GameControl : ReactiveUserControl<MainWindowViewModel>
{
    public GameControl()
    {
        InitializeComponent();
        DescriptionExpander.IsExpanded = false;
    }

    public void Reset() => GameMap.Reset();
}
