namespace ZaggyCode.Core.Languages.Options;

public sealed class LuaPathsOptions
{
    public required string RegisterIoLuaPath { get; set; }
    public required string RegisterIncorrectlyWroteNameCheckerLuaPath { get; set; }
    public required string RegisterRobotLuaPath { get; set; }
}