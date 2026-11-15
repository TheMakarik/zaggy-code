namespace ZaggyCode.Modules.Data;

//#:NO_AI
internal sealed record SettingsStorageTarget(
    string Name,
    BackgroundTaskKind FlashKind,
    BackgroundTaskKind RevertKind,
    BackgroundTaskKind DefaultKind,
    ISettingsStorage Storage);
