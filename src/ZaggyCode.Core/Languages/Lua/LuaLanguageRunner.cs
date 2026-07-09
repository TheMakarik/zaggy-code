namespace ZaggyCode.Core.Languages.Lua;

[LanguageExtension(".lua")]
public sealed class LuaLanguageRunner(
    IUserStorage userStorage,
    ILogger<LuaLanguageRunner> logger,
    IOptions<SpeedMillisecondsOptions> speedOptions, 
    IOptions<LuaPathsOptions> luaOptions) : ILanguageRunner
{
    private readonly NLua.Lua _lua = new();
    
    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    
    public void RedirectIoStreams(TextReader input, TextWriter output)
    {
        _lua["__clr_input"] = input;
        _lua["__clr_output"] = output;
        _lua["TABLE_CONTENT_CHECKER"] = userStorage.Current.LuaData.UseTableContentCheckerByDefault;
        _lua.MaximumRecursion = userStorage.Current.LuaData.MaxRecursion;

        _lua.DoFile(luaOptions.Value.RegisterIoLuaPath);
        _lua.DoFile(luaOptions.Value.RegisterRobotLuaPath);
        _lua.DoFile(luaOptions.Value.RegisterIncorrectlyWroteNameCheckerLuaPath);
    }

    public void Execute(string code, ExecutionSpeed speed, IRobotExecutor executor)
    {
        //var speedMilliseconds = (int)options.GetType().GetProperty(speed.ToString()!)!.GetValue(speed)!;
        _lua.State.Encoding = Encoding.UTF8;
        _lua.DoString(code);
    }
    
    public void Dispose()
    {
        _lua.Dispose();
    }
}