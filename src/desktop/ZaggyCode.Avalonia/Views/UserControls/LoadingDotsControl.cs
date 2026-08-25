namespace ZaggyCode.Avalonia.Views.UserControls;

public sealed class LoadingDotsControl : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<LoadingDotsControl, string>(nameof(Text), "Загрузка");

    public static readonly StyledProperty<int> MaxDotCountProperty =
        AvaloniaProperty.Register<LoadingDotsControl, int>(nameof(MaxDotCount), 3);

    private const int AnimationIntervalMilliseconds = 400;

    private readonly TextBlock _textBlock;
    private readonly DispatcherTimer _timer;
    private int _currentDotCount;

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public int MaxDotCount
    {
        get => GetValue(MaxDotCountProperty);
        set => SetValue(MaxDotCountProperty, value);
    }

    public LoadingDotsControl()
    {
        _textBlock = new TextBlock { Text = Text };
        Content = _textBlock;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(AnimationIntervalMilliseconds) };
        _timer.Tick += (_, _) => UpdateDots();

        AttachedToVisualTree += (_, _) => _timer.Start();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty || change.Property == MaxDotCountProperty)
            RenderText();
    }

    private void UpdateDots()
    {
        var maxDots = Math.Max(1, MaxDotCount);
        _currentDotCount = (_currentDotCount + 1) % (maxDots + 1);

        RenderText();
    }

    private void RenderText() =>
        _textBlock.Text = Text + new string('.', Math.Clamp(_currentDotCount, 0, Math.Max(1, MaxDotCount)));
}
