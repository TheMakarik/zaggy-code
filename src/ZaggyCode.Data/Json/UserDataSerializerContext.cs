using System.Text.Json.Serialization;
using ZaggyCode.Data.Model;

namespace ZaggyCode.Data.Json;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UserData))]
public sealed partial class UserDataSerializerContext :  JsonSerializerContext;