namespace ZaggyCode.Avalonia.Views.TerminalEngine.Exceptions;

public class InvalidByteException(byte invalidByte, string message) : Exception(message)
{
    public byte InvalidByte { get; } = invalidByte;
}
