namespace ZaggyCode.Modules.Data.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PythonSettings))]
public sealed partial class PythonSettingsSerializerContext : JsonSerializerContext;
