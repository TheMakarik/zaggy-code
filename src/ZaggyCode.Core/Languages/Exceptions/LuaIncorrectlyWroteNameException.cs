namespace ZaggyCode.Core.Languages.Exceptions;

public sealed class LuaIncorrectlyWroteNameException(string actual, string suggestion) : Exception
{
    public override string Message => $"{Actual} is not exists. Maybe you mean {Suggestion}";

    public string Actual { get; } = actual;
    public string Suggestion { get; } = suggestion;
}