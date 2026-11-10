namespace ZaggyCode.Modules.Data.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ViewSettings))]
public sealed partial class ViewSettingsSerializerContext : JsonSerializerContext;
