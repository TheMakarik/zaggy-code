namespace ZaggyCode.Avalonia.Options;

public sealed class MapAssetsOptions
{
    public required string TilesPath 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string Coin 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallFull 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallDown 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallUp 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallLeft 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallRight 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallLeftDown 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallLeftUp 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallRightDown 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallRightUp 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallLeftUpDown 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string WallRightUpDown 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}