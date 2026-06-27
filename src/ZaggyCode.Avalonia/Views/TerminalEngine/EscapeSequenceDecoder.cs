using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ZaggyCode.Avalonia.Views.TerminalEngine;

public abstract class EscapeSequenceDecoder : ITerminalDecoder
{
    private enum State
    {
        Ground,         // Обычный текст
        Escape,         // Получили \x1b
        CsiEntry,       // Получили [ после Escape
        CsiParam,       // Читаем цифры или ;

        OscEntry,       // Сразу после ]
        OscParam,       // Читаем номер команды (то самое '0')
        OscString,      // Читаем сам текст заголовка
        OscTermination  // Если встретили ESC, проверяем на \ (для ST)
    }

    public const byte EscapeCharacter = 0x1B;
    public const byte XonCharacter = 17;
    public const byte XoffCharacter = 19;

    public const byte LeftBracketCharacter = 0x5B;
    public const byte RightBracketCharacter = 0x5D;
    public const byte SemicolonCharacter = 0x3B;
    public const byte QuestionMarkCharacter = 0x3F;
    public const byte BelCharacter = 0x07;
    public const byte OscTerminateCharacter = (byte)'\\';

    private readonly List<byte> oscPayload = [];
    private readonly List<int> paramBuffer = [];
    private int paramAccumulator = 0;
    private bool hasParam = false;
    private bool privateMode = false;

    // Buffer for accumulating bytes for multi-byte character decoding
    private List<byte> characterBuffer = [];

    private State state = State.Ground;
    private bool supportXonXoff = true;
    private bool xOffReceived = false;

    private bool disposing;
    private bool disposed;

    public Encoding Encoding
    {
        get => field;
        set => field = value;
    }

    protected EscapeSequenceDecoder()
    {
        Encoding = Encoding.UTF8;
    }

    public void Write(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            throw new ArgumentException("Input can not process an empty array.");

        foreach (byte suquenceByte in data)
        {
            try
            {
                ProcessByte(suquenceByte);
            }
            catch (Exception exc)
            {
                while (exc.InnerException != null)
                    exc = exc.InnerException;

                Debug.WriteLine(exc.Message);
                Drain();
            }
        }
    }

    private void Drain()
    {
        FlushCharacterBuffer();
        state = State.Ground;
        paramAccumulator = 0;
        paramBuffer.Clear();
        privateMode = false;
        oscPayload.Clear();
    }

    private void ProcessByte(byte suquenceByte)
    {
        switch (state)
        {
            case State.Ground:
                {
                    ProcessGround(suquenceByte);
                    break;
                }

            case State.Escape:
                {
                    ProcessEscape(suquenceByte);
                    break;
                }

            case State.CsiEntry:
                {
                    ProcessCsiEntry(suquenceByte);
                    break;
                }

            case State.CsiParam:
                {
                    ProcessCsiParam(suquenceByte);
                    break;
                }

            case State.OscEntry:
                {
                    ProcessOscEntry(suquenceByte);
                    break;
                }

            case State.OscParam:
                {
                    ProcessOscParam(suquenceByte);
                    break;
                }

            case State.OscString:
                {
                    ProcessOscString(suquenceByte);
                    break;
                }

            case State.OscTermination:
                {
                    ProcessOscTermination(suquenceByte);
                    break;
                }
        }
    }

    private void ProcessGround(byte b)
    {
        if (b == EscapeCharacter)
        {
            // Flush any accumulated bytes before processing escape sequence
            FlushCharacterBuffer();
            state = State.Escape;
            return;
        }

        // Accumulate bytes for multi-byte character decoding
        characterBuffer.Add(b);

        // Try to decode accumulated bytes
        TryDecodeCharacters();
    }

    private void TryDecodeCharacters()
    {
        if (characterBuffer.Count == 0)
            return;

        // Try to decode using the current encoding
        Decoder decoder = Encoding.GetDecoder();
        int maxCharCount = Encoding.GetMaxCharCount(characterBuffer.Count);
        char[] charBuffer = new char[maxCharCount];

        decoder.Convert(
            characterBuffer.ToArray(), 0, characterBuffer.Count,
            charBuffer, 0, maxCharCount, false,
            out int bytesUsed, out int charsProduced, out bool completed);

        if (charsProduced > 0)
        {
            // Successfully decoded some characters
            OnCharacters(charBuffer.AsSpan(0, charsProduced));

            // Remove used bytes from buffer
            if (bytesUsed > 0)
            {
                characterBuffer.RemoveRange(0, bytesUsed);
            }
        }

        // If not completed and buffer is getting too large, flush what we can
        if (!completed && characterBuffer.Count > 4)
        {
            // Try to decode at least one character
            decoder.Convert(
                characterBuffer.ToArray(), 0, characterBuffer.Count,
                charBuffer, 0, maxCharCount, true,
                out bytesUsed, out charsProduced, out completed);

            if (charsProduced > 0)
            {
                OnCharacters(charBuffer.AsSpan(0, charsProduced));
                if (bytesUsed > 0)
                {
                    characterBuffer.RemoveRange(0, bytesUsed);
                }
            }
            else
            {
                // If we still can't decode, output as replacement character and clear buffer
                OnCharacters("\uFFFD".AsSpan());
                characterBuffer.Clear();
            }
        }
    }

    private void FlushCharacterBuffer()
    {
        if (characterBuffer.Count == 0)
            return;

        // Force decode remaining bytes
        Decoder decoder = Encoding.GetDecoder();
        int maxCharCount = Encoding.GetMaxCharCount(characterBuffer.Count);
        char[] charBuffer = new char[maxCharCount];

        decoder.Convert(
            characterBuffer.ToArray(), 0, characterBuffer.Count,
            charBuffer, 0, maxCharCount, true,
            out int bytesUsed, out int charsProduced, out bool completed);

        if (charsProduced > 0)
        {
            OnCharacters(charBuffer.AsSpan(0, charsProduced));
        }
        else if (characterBuffer.Count > 0)
        {
            // If we can't decode, output replacement character
            OnCharacters("\uFFFD".AsSpan());
        }

        characterBuffer.Clear();
    }

    private void AccumulateParam(byte suquenceByte)
    {
        hasParam = true;
        paramAccumulator *= 10;
        paramAccumulator += suquenceByte - 0x30;
    }

    private void PushParam()
    {
        if (!hasParam)
            return;

        paramBuffer.Add(paramAccumulator);
        paramAccumulator = 0;
        hasParam = false;
    }

    private void ProcessEscape(byte suquenceByte)
    {
        switch (suquenceByte)
        {
            case LeftBracketCharacter:
                {
                    state = State.CsiEntry;
                    break;
                }

            case RightBracketCharacter:
                {
                    state = State.OscEntry;
                    break;
                }

            default:
                {
                    state = State.Ground;
                    break;
                }
        }
    }

    private void ProcessCsiEntry(byte suquenceByte)
    {
        switch (suquenceByte)
        {
            case SemicolonCharacter:
                {
                    PushParam();
                    state = State.CsiParam;
                    break;
                }

            case QuestionMarkCharacter:
                {
                    privateMode = true;
                    break;
                }

            case var _ when IsDigit(suquenceByte):
                {
                    AccumulateParam(suquenceByte);
                    state = State.CsiParam;
                    break;
                }

            case var _ when IsCommand(suquenceByte):
                {
                    PushParam();
                    DispatchCsi(suquenceByte);
                    break;
                }

            default:
                throw new InvalidByteException(suquenceByte, "Unknown byte during CSI entry processing");
        }
    }

    private void ProcessCsiParam(byte suquenceByte)
    {
        switch (suquenceByte)
        {
            case SemicolonCharacter:
                {
                    PushParam();
                    state = State.CsiParam;
                    break;
                }

            case var _ when IsDigit(suquenceByte):
                {
                    AccumulateParam(suquenceByte);
                    state = State.CsiParam;
                    break;
                }

            case var _ when IsCommand(suquenceByte):
                {
                    PushParam();
                    DispatchCsi(suquenceByte);
                    break;
                }

            default:
                throw new InvalidByteException(suquenceByte, "Unknown byte during CSI param processing");
        }
    }

    private void ProcessOscEntry(byte suquenceByte)
    {
        switch (suquenceByte)
        {
            case SemicolonCharacter:
                {
                    PushParam();
                    state = State.OscParam;
                    break;
                }

            case QuestionMarkCharacter:
                {
                    privateMode = true;
                    break;
                }

            case var _ when IsDigit(suquenceByte):
                {
                    AccumulateParam(suquenceByte);
                    state = State.OscParam;
                    break;
                }

            case var _ when IsCommand(suquenceByte):
                {
                    PushParam();
                    DispatchOsc();
                    break;
                }

            default:
                throw new InvalidByteException(suquenceByte, "Unknown byte during OSC entry processing");
        }
    }

    private void ProcessOscParam(byte suquenceByte)
    {
        switch (suquenceByte)
        {
            case SemicolonCharacter:
                {
                    PushParam();
                    state = State.OscString;
                    break;
                }

            case BelCharacter:
                {
                    DispatchOsc();
                    break;
                }

            case var _ when IsDigit(suquenceByte):
                {
                    AccumulateParam(suquenceByte);
                    state = State.OscParam;
                    break;
                }

            case var _ when IsCommand(suquenceByte):
                {
                    PushParam();
                    DispatchOsc();
                    break;
                }

            default:
                throw new InvalidByteException(suquenceByte, "Unknown byte during OSC param processing");
        }
    }

    private void ProcessOscString(byte suquenceByte)
    {
        switch (suquenceByte)
        {
            case BelCharacter:
                {
                    DispatchOsc();
                    state = State.Ground;
                    break;
                }

            case EscapeCharacter:
                {
                    state = State.OscTermination;
                    return;
                }

            default:
                {
                    oscPayload.Add(suquenceByte);
                    break;
                }
        }
    }

    private void ProcessOscTermination(byte suquenceByte)
    {
        switch (suquenceByte)
        {
            case OscTerminateCharacter:
                {
                    DispatchOsc();
                    state = State.Ground;
                    break;
                }

            default:
                {
                    oscPayload.Add(suquenceByte);
                    state = State.OscString;
                    break;
                }
        }
    }

    private void DispatchCsi(byte command)
    {
        ProcessCsiCommand(command, paramBuffer.ToArray(), privateMode);
        Drain();
    }

    private void DispatchOsc()
    {
        ProcessOscCommand(paramBuffer.ToArray(), Encoding.UTF8.GetString(oscPayload.ToArray()));
        Drain();
    }

    private static bool IsDigit(byte b) => b >= 0x30 && b <= 0x39;
    private static bool IsCommand(byte b) => b >= 0x40 && b <= 0x7E;

    protected abstract void OnCharacters(ReadOnlySpan<char> characters);
    protected abstract void ProcessCsiCommand(byte command, ReadOnlySpan<int> parameters, bool privateMode);
    protected abstract void ProcessOscCommand(ReadOnlySpan<int> parameters, string payload);

    protected void ThrowIsDisposed()
    {
        if (disposed || disposing)
            throw new ObjectDisposedException(GetType().Name);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        Drain();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
            return;

        Dispose(true);
        GC.SuppressFinalize(this);
        disposed = true;
    }
}
