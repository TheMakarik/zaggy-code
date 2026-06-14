using Microsoft.Extensions.Options;
using ZaggyCode.Data.Model;

namespace ZaggyCode.Data.Options;

public sealed class DefaultUser
{
    public required UserData User { get; set; }
}