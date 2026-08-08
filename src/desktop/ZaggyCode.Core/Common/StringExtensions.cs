namespace ZaggyCode.Core.Common;

public static class StringExtensions
{
    public static string TrimDirectorySeparator(this string path)
    {
        return path[^1] == Path.DirectorySeparatorChar ? path[..^1] : path;
    }
    
    public static string FromPascalCaseToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var builder = new StringBuilder(value.Length + 10);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (char.IsUpper(character))
            {
                if (i > 0)
                    builder.Append('_');

                builder.Append(char.ToLowerInvariant(character));
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
