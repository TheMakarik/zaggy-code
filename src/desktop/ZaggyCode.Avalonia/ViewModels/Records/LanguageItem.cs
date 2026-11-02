using Language = ZaggyCode.Core.LanguageRuntime.Enums.Language;

namespace ZaggyCode.Avalonia.ViewModels.Records;

public sealed record LanguageItem(Language Value, string Name, MaterialIconKind IconKind);
