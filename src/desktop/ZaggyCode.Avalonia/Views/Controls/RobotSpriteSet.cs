namespace ZaggyCode.Avalonia.Views.Controls;

internal sealed class RobotSpriteSet
{
    public IImage Front { get; private init; } = null!;
    public IImage Back { get; private init; } = null!;
    public IImage Left { get; private init; } = null!;
    public IImage Right { get; private init; } = null!;
    public IImage Dead { get; private init; } = null!;

    public static RobotSpriteSet Load()
    {
        return new RobotSpriteSet
        {
            Front = LoadSvg("zaggy-side-front.svg"),
            Back = LoadSvg("zaggy-side-back.svg"),
            Left = LoadSvg("zaggy-side-left.svg"),
            Right = LoadSvg("zaggy-side-right.svg"),
            Dead = LoadSvg("zaggy-emotion-sad.svg")
        };
    }

    private static IImage LoadSvg(string fileName)
    {
        string path = Path.Join(AppContext.BaseDirectory, "Assets", "Zaggy", fileName);
        SvgSource source = SvgSource.Load(path);
        return new SvgImage { Source = source };
    }
}
