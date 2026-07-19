using AvaloniaEdit.Rendering;

namespace ZaggyCode.Avalonia.Views.TextEditing;

public class LineHighlighter(int lineNumber, Color color) : IBackgroundRenderer
{
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        var line = textView.Document.GetLineByNumber(lineNumber);
        var rects = BackgroundGeometryBuilder.GetRectsForSegment(textView, line);
        
        foreach (var rect in rects)
        {
            var fullLineRect = new Rect(
                textView.Bounds.X,
                rect.Y,
                textView.Bounds.Width,
                rect.Height
            );
            
            drawingContext.DrawRectangle(new SolidColorBrush(color), null, fullLineRect);
        }
    }

    public KnownLayer Layer => KnownLayer.Background;
}