namespace ZaggyCode.Core.Data.Model;

//#:NO_AI
public sealed record SettingsStorageStep(
    string Description,
    Func<ValueTask> Action);
