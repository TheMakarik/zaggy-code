namespace ZaggyCode.Languages.Options;

public sealed class RobotsFunctions
{
    public required string MoveLeft { get; set; }
    public required string MoveRight { get; set; }
    public required string MoveUp { get; set; }
    public required string MoveDown { get; set; }
    
    public required string IsWallFromLeft { get; set; }
    public required string IsWallFromRight { get; set; }
    public required string IsWallFromUp { get; set; }
    public required string IsWallFromDown { get; set; }
    
    public required string Draw { get; set; }
}