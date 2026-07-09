namespace ZaggyCode.Languages.Exceptions;

public sealed class IncorrectlyWroteNameException(string actual, string suggestion) : Exception
{
    public override string Message => $"{Actual} is not exists. Maybe you mean {Suggestion}";

    public string Actual { get; } = actual;
    public string Suggestion { get; } = suggestion;
}