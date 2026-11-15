namespace ZaggyCode.Modules.Data;

//#:NO_AI
internal sealed record SettingsStorageStep(
    string Description,
    BackgroundTaskKind OperationKind,
    Func<ValueTask> Action);
