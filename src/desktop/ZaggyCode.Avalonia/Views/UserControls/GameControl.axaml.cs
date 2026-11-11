namespace ZaggyCode.Avalonia.Views.UserControls;

public partial class GameControl : ReactiveUserControl<MainWindowViewModel>
{
    public GameControl()
    {
        InitializeComponent();
    }

    public MapControl Map => GameMap;

    public void Reset() => GameMap.Reset();
}
