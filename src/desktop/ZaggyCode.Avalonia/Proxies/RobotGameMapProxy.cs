using ZaggyCode.Core.Proxies.Interfaces;

namespace ZaggyCode.Avalonia.Proxies;

public sealed class RobotGameMapProxy : IRobotGameMapProxy
{
    private readonly Lock _attachLock = new();
    private MapControl? _map;

    public void Attach(MapControl map)
    {
        using var scope = _attachLock.EnterScope();
        _map = map;
    }

    public void Reset() => Run(map => map.Reset());

    public void SetRobotPosition(int x, int y, RobotDirection direction)
        => Run(map => map.SetRobotPosition(x, y, direction));

    public void CollectCoin(int x, int y)
        => Run(map => map.SetCellCollected(x, y, true));

    public void MarkDrawn(int x, int y)
        => Run(map => map.SetCellDrawn(x, y, true));

    public void KillRobot(int x, int y, RobotDirection? attemptedDirection, RobotDeadType deadType)
        => Run(map => map.PlayCollisionAnimation(attemptedDirection, x, y));
    private void Run(System.Action<MapControl> action)
    {
        MapControl? map;
        using (var scope = _attachLock.EnterScope())
            map = _map;

        if (map is null)
            return;

        action(map);
    }
}
