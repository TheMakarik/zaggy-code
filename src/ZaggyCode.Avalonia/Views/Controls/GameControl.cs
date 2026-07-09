using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ZaggyCode.Games.Events;
using ZaggyCode.Games.Models;

namespace ZaggyCode.Avalonia.Views.Controls;

public sealed class GameControl : ContentControl
{
    private Game? _game;
    private UniformGrid _grid = new UniformGrid();

    public static readonly DirectProperty<GameControl, Game> GameProperty = AvaloniaProperty.RegisterDirect<GameControl, Game>(
        nameof(Game), o => o.Game, (o, v) => o.Game = v);

    public Game Game
    {
        get => _game;
        set => SetAndRaise(GameProperty, ref _game, value);
    }

    private RobotEvents _robotEvents;

    public static readonly DirectProperty<GameControl, RobotEvents> RobotEventsProperty = AvaloniaProperty.RegisterDirect<GameControl, RobotEvents>(
        nameof(RobotEvents), o => o.RobotEvents, (o, v) => o.RobotEvents = v);

    public RobotEvents RobotEvents
    {
        get => _robotEvents;
        set => SetAndRaise(RobotEventsProperty, ref _robotEvents, value);
    }
    
}