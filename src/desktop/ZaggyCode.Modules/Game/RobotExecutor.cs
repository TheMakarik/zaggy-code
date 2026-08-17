namespace ZaggyCode.Modules.Game;

public sealed class RobotExecutor(Map map) : IRobotExecutor
{
    private readonly Point _robotPoint = map.Points.First(p => p.IsSpawn);
    public EventHandler<RobotPointUpdatedEventArgs> RobotPointUpdated { get; set; }

    public void MoveUp()
    {
        
    }

    public void MoveRight()
    {
        throw new NotImplementedException();
    }

    public void MoveDown()
    {
        throw new NotImplementedException();
    }

    public void MoveLeft()
    {
        throw new NotImplementedException();
    }

    public void FillCell()
    {
        throw new NotImplementedException();
    }

    public bool IsCellFilled()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromUp()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromDown()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromLeft()
    {
        throw new NotImplementedException();
    }

    public bool IsWallFromRight()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}