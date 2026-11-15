namespace ZaggyCode.Modules.Data;

//#:NO_AI
internal sealed record SettingsStorageStep(
    string Name,
    BackgroundTaskKind OperationKind,
    Func<ValueTask> Action);
