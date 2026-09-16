namespace ZaggyCode.Modules.Data.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ThemeSettings))]
public sealed partial class ThemeSettingsSerializerContext : JsonSerializerContext;
