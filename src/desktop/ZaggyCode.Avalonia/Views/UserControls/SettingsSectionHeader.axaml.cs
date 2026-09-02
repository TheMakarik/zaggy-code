namespace ZaggyCode.Avalonia.Views.UserControls;

public sealed partial class SettingsSectionHeader : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsSectionHeader, string>(nameof(Title));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<SettingsSectionHeader, string>(nameof(Description));

    public static readonly StyledProperty<MaterialIconKind> IconKindProperty =
        AvaloniaProperty.Register<SettingsSectionHeader, MaterialIconKind>(nameof(IconKind));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public MaterialIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public SettingsSectionHeader()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleProperty)
            TitleText.Text = Title;
        else if (change.Property == DescriptionProperty)
            DescriptionText.Text = Description;
        else if (change.Property == IconKindProperty)
            IconControl.Kind = IconKind;
    }
}
