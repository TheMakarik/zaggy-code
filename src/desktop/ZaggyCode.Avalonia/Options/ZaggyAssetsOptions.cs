namespace ZaggyCode.Avalonia.Options;

public sealed class ZaggyAssetsOptions
{
    public required string IconPath
    {
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string LogoPath
    {
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string EmotionAngry 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string EmotionLike 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
    
    public required string EmotionQuestion 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string EmotionLove 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string EmotionSad 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string EmotionShock 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideFront 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideBack 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideLeft 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideRight 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideFrontLeft 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideFrontRight 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideBackLeft 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideBackRight 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideFrontLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideBackLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideLeftLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideRightLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideFrontLeftLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideFrontRightLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideBackLeftLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }

    public required string ZaggySideBackRightLaggy 
    { 
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}