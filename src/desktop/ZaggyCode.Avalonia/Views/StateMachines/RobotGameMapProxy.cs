using ZaggyCode.Avalonia.Views.Records;

namespace ZaggyCode.Avalonia.Views.StateMachines;

internal sealed class RobotGameMapProxy(MapView mapView, MapRobotStateMachine stateMachine) : IRobotGameMapProxy
{
    private readonly MapView _mapView = mapView;
    private readonly MapRobotStateMachine _stateMachine = stateMachine;

    public void MoveRobot(int newX, int newY)
    {
        RunOnUiThread(() =>
        {
            _stateMachine.SetLogicalCell(newX, newY);
            _mapView.MoveRobotTo(newX, newY);
        });
    }

    public void MoveRobot(Direction direction)
    {
        RunOnUiThread(() =>
        {
            CellOffset offset = direction switch
            {
                Direction.Up => new CellOffset(0, -1),
                Direction.Down => new CellOffset(0, 1),
                Direction.Left => new CellOffset(-1, 0),
                Direction.Right => new CellOffset(1, 0),
                _ => new CellOffset(0, 0)
            };

            var current = new Cell(_stateMachine.LogicalColumn, _stateMachine.LogicalRow);
            var next = current + offset;

            _stateMachine.SetDirection(direction);
            _stateMachine.SetLogicalCell(next.Column, next.Row);
            _mapView.MoveRobotTo(next.Column, next.Row);
        });
    }

    public void FillPoint(int x, int y)
    {
        RunOnUiThread(() =>
        {
            _mapView.SetCellPainted(x, y, true);
            _mapView.FlashCell(x, y);
        });
    }

    public void RobotDead(RobotDeadType deadType, Direction? whenMovedTo = null)
    {
        RunOnUiThread(() =>
        {
            _mapView.PlayDeathAnimation(whenMovedTo);
        });
    }

    private static void RunOnUiThread(System.Action action)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(action);
            return;
        }

        action();
    }
}
