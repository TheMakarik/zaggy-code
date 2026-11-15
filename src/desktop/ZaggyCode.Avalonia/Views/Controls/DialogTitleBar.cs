using Material.Icons.Avalonia;

namespace ZaggyCode.Avalonia.Views.Controls;

public sealed class DialogTitleBar : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DialogTitleBar, string?>(nameof(Title));

    public static readonly StyledProperty<MaterialIconKind?> IconKindProperty =
        AvaloniaProperty.Register<DialogTitleBar, MaterialIconKind?>(nameof(IconKind));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public MaterialIconKind? IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public event EventHandler? CloseRequested;

    private readonly TextBlock _titleText;
    private readonly MaterialIcon _titleIcon;
    private readonly Button _minimizeButton;
    private readonly Button _closeButton;

    public DialogTitleBar()
    {
        Classes.Add("header-bar");
        Padding = new Thickness(8, 4);

        _titleIcon = new MaterialIcon
        {
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            Width = 18,
            Height = 18
        };

        _titleText = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center
        };

        var titlePanel = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center
        };
        titlePanel.Children.Add(_titleIcon);
        titlePanel.Children.Add(_titleText);

        _minimizeButton = CreateWindowButton(MaterialIconKind.WindowMinimize, "Свернуть");
        _minimizeButton.Click += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is Window window)
                window.WindowState = WindowState.Minimized;
        };

        _closeButton = CreateWindowButton(MaterialIconKind.WindowClose, "Закрыть");
        _closeButton.Classes.Add("close");
        _closeButton.Click += (_, _) =>
        {
            if (CloseRequested is not null)
            {
                CloseRequested.Invoke(this, EventArgs.Empty);
                return;
            }

            (TopLevel.GetTopLevel(this) as Window)?.Close();
        };

        var buttonsPanel = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 4
        };
        buttonsPanel.Children.Add(_minimizeButton);
        buttonsPanel.Children.Add(_closeButton);

        var layout = new Grid();
        layout.ColumnDefinitions = new ColumnDefinitions("*,Auto");
        Grid.SetColumn(titlePanel, 0);
        Grid.SetColumn(buttonsPanel, 1);
        layout.Children.Add(titlePanel);
        layout.Children.Add(buttonsPanel);

        Child = layout;

        PointerPressed += (_, e) =>
        {
            if (TopLevel.GetTopLevel(this) is not Window window || !e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
                return;

            window.BeginMoveDrag(e);
            e.Handled = true;
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleProperty)
            _titleText.Text = change.NewValue as string;

        if (change.Property == IconKindProperty)
        {
            _titleIcon.Kind = change.NewValue as MaterialIconKind? ?? default;
            _titleIcon.IsVisible = change.NewValue is not null;
        }
    }

    private static Button CreateWindowButton(MaterialIconKind iconKind, string toolTip)
    {
        var button = new Button { Content = new MaterialIcon { Kind = iconKind } };
        ToolTip.SetTip(button, toolTip);
        button.Classes.Add("icon-button");
        button.Classes.Add("window-control");
        return button;
    }
}
