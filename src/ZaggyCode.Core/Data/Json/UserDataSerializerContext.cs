namespace ZaggyCode.Core.Data.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UserData))]
public sealed partial class UserDataSerializerContext :  JsonSerializerContext;