namespace ZaggyCode.Avalonia.Options;

public sealed class MapAssetsOptions
{
    public required string Coin
    {
        get;
        set => field = Environment.ExpandEnvironmentVariables(value);
    }
}
