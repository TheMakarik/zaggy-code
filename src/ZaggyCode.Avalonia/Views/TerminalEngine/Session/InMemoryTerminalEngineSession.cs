namespace ZaggyCode.Avalonia.Views.TerminalEngine.Session;

/// <summary>
/// An in-memory terminal session that emulates a basic command line shell (CLI).
/// </summary>
public class InMemoryTerminalEngineSession : InMemoryTerminalSession
{
    private readonly StringBuilder _currentInputBuffer = new StringBuilder();
    private readonly string _prompt = "rikitav-shell> ";

    private TaskCompletionSource<string>? _readLineTcs;

    public InMemoryTerminalEngineSession() : base()
    {
        Title = null!;
    }

    public Task<string> ReadLineAsync()
    {
        if (_readLineTcs != null)
            return _readLineTcs.Task;

        _readLineTcs = new TaskCompletionSource<string>();
        return _readLineTcs.Task;
    }

    public override void Write(ReadOnlySpan<byte> data)
    {
        string inputStr = InputEncoding.GetString(data);

        foreach (char c in inputStr)
        {
            if (c == '\r' || c == '\n')
            {
                FeedOutput("\r\n");

                string input = _currentInputBuffer.ToString();
                _currentInputBuffer.Clear();

                if (_readLineTcs != null)
                {
                    _readLineTcs.SetResult(input);
                    _readLineTcs = null;
                    continue;
                }

                string command = input.Trim();
                if (string.IsNullOrEmpty(command))
                {
                    PrintPrompt();
                    continue;
                }

                _ = ExecuteCommandAsync(command);
            }
            else if (c == '\b' || c == (char)127)
            {
                if (_currentInputBuffer.Length == 0)
                    continue;

                _currentInputBuffer.Remove(_currentInputBuffer.Length - 1, 1);
                FeedOutput("\b \b");
            }
            else if (!char.IsControl(c))
            {
                _currentInputBuffer.Append(c);
                FeedOutput(c.ToString());
            }
        }
    }

    public void PrintPrompt()
    {
        FeedOutput("\x1b[32m" + _prompt + "\x1b[0m");
    }

    private async Task ExecuteCommandAsync(string rawCommand)
    {
        string[] parts = rawCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "help":
                {
                    FeedOutput("Available commands:\r\n");
                    FeedOutput("  help  - Show this message\r\n");
                    FeedOutput("  echo  - Print text to terminal\r\n");
                    FeedOutput("  read  - Interactive input testing\r\n");
                    FeedOutput("  clear - Clear the screen\r\n");
                    FeedOutput("  exit  - Disconnect session\r\n");
                    break;
                }

            case "echo":
                {
                    if (parts.Length > 1)
                    {
                        string text = string.Join(' ', parts[1..]);
                        FeedOutput(text + "\r\n");
                        break;
                    }

                    FeedOutput("\r\n");
                    break;
                }

            case "read":
                {
                    FeedOutput("Enter your name: ");
                    string name = await ReadLineAsync();
                    FeedOutput($"\x1b[33mHello, {name}!\x1b[0m\r\n");
                    break;
                }

            case "clear":
                {
                    FeedOutput("\x1b[33J\x1b[H\x1b[2J");
                    break;
                }

            case "exit":
                {
                    FeedOutput("Goodbye!\r\n");
                    Disconnect();
                    return;
                }

            default:
                {
                    FeedOutput($"\x1b[31mCommand not found: {cmd}\x1b[0m\r\n");
                    break;
                }
        }

        PrintPrompt();
    }
}
