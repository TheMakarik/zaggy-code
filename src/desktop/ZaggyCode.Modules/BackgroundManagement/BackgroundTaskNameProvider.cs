namespace ZaggyCode.Modules.BackgroundManagement;

//#:NO_AI
public sealed class BackgroundTaskNameProvider(IOptions<BackgroundTaskNameOptions> options) : IBackgroundTaskNameProvider
{
    public string GetName(BackgroundTaskKind kind)
        => options.Value.Names.TryGetValue(kind, out var name) ? name : kind.ToString();
}
