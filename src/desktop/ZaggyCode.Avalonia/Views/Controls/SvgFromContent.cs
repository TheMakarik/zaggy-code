namespace ZaggyCode.Avalonia.Views.Controls;

public class SvgFromContent : ContentControl
{
    public static readonly StyledProperty<string> PathProperty = AvaloniaProperty.Register<SvgFromContent, string>(
        nameof(Path));

    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public SvgFromContent()
    {
        this.WhenPropertyChanged(p => p.Path).Subscribe(path =>
        {
            if (string.IsNullOrWhiteSpace(path.Value))
                return;
            var content = new global::Avalonia.Svg.Skia.Svg((Uri?)null!) { SvgSource = SvgSource.Load(path.Value) };
            this.Content = content;
        });
    }
}