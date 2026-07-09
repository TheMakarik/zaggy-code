namespace ZaggyCode.Avalonia.Views.TerminalEngine.Exceptions;

public sealed class InvalidCommandException(byte invalidByte, string parameter)
    : InvalidByteException(invalidByte, string.Format("Invalid command {0:X2} '{1}', parameter = \"{2}\"", invalidByte, (char)invalidByte, parameter))
{
    public byte Command => InvalidByte;
    public string Paramter { get; } = parameter;
}