namespace ZaggyCode.Modules.Data.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CSharpSettings))]
public sealed partial class CSharpSettingsSerializerContext : JsonSerializerContext;
