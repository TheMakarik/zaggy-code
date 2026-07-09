namespace ZaggyCode.Avalonia.Views.TerminalEngine.Exceptions;

public sealed class InvalidParameterException(byte invalidByte, string parameter)
    : InvalidByteException(invalidByte, string.Format("Invalid parameter for command {0:X2} '{1}', parameter = \"{2}\"", invalidByte, (char)invalidByte, parameter))
{
    public byte Command => InvalidByte;
    public string Paramter { get; } = parameter;
}
