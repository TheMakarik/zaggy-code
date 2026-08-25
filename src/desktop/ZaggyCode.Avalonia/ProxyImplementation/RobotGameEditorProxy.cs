namespace ZaggyCode.Avalonia.ProxyImplementation;

public sealed class RobotGameEditorProxy : IRobotGameEditorProxy
{
    private const string ExecutionLineColorResource = "ForegroundDarkColor";

    private readonly Lock _attachLock = new();
    private TextEditor? _editor;
    private LineHighlighter? _currentHighlighter;

    public void Attach(TextEditor editor)
    {
        using var scope = _attachLock.EnterScope();
        _editor = editor;
    }
    
    public void SetCurrentExecutionLine(int line)
        => Dispatcher.UIThread.Post(() => SetLine(line));

    private void SetLine(int line)
    {
        TextEditor? editor;
        using (var scope = _attachLock.EnterScope())
            editor = _editor;

        if (editor is null)
            return;

        var textView = editor.TextArea.TextView;

        if (_currentHighlighter is not null)
        {
            textView.BackgroundRenderers.Remove(_currentHighlighter);
            _currentHighlighter = null;
        }

        if (line <= 0)
        {
            textView.Redraw();
            return;
        }

        if (!Application.Current!.TryFindResource(ExecutionLineColorResource, out var resource) || resource is not Color color)
        {
            Debug.Assert(false, $"Resource {ExecutionLineColorResource} must be a Color");
            return;
        }

        _currentHighlighter = new LineHighlighter(line, color);
        textView.BackgroundRenderers.Add(_currentHighlighter);
        textView.Redraw();
    }
}
