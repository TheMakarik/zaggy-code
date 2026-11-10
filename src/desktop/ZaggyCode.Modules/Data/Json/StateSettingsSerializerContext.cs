namespace ZaggyCode.Modules.Data.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(StateSettings))]
public sealed partial class StateSettingsSerializerContext : JsonSerializerContext;
