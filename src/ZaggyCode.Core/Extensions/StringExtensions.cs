namespace ZaggyCode.Core.Extensions;

public static class StringExtensions
{
    public static string TrimDirectorySeparator(this string path)
    {
        return path[^1] == Path.DirectorySeparatorChar ? path[..^1] : path;
    }
}