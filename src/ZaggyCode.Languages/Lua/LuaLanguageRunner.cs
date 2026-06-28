using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZaggyCode.Games.Events;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Languages.Attributes;
using ZaggyCode.Languages.Enums;
using ZaggyCode.Languages.EventArgs;
using ZaggyCode.Languages.Interfaces;
using ZaggyCode.Languages.Options;
using ZaggyCode.Shared.Attributes;

namespace ZaggyCode.Languages.Lua;

[LanguageExtension(".lua")]
public sealed class LuaLanguageRunner : ILanguageRunner
{
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public EventHandler<DebugLineUpdatedEventArgs>? DebugLineUpdated { get; set; }
    public void RedirectIoStreams(TextReader input, TextWriter output)
    {
        throw new NotImplementedException();
    }

    public void Execute(string code, ExecutionSpeed speed, IRobotMover mover)
    {
        throw new NotImplementedException();
    }
}