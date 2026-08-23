using Material.Icons;
using Material.Icons.Avalonia;

namespace ZaggyCode.Avalonia.Views.Controls;

public sealed class DialogTitleBar : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DialogTitleBar, string?>(nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // Подписчик может перехватить закрытие (например Close(false) для диалога подтверждения),
    // иначе панель просто закрывает окно.
    public event EventHandler? CloseRequested;

    private readonly TextBlock _titleText;
    private readonly Button _minimizeButton;
    private readonly Button _closeButton;

    public DialogTitleBar()
    {
        Classes.Add("header-bar");
        Padding = new Thickness(12, 8);

        _titleText = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center
        };

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
        Grid.SetColumn(_titleText, 0);
        Grid.SetColumn(buttonsPanel, 1);
        layout.Children.Add(_titleText);
        layout.Children.Add(buttonsPanel);

        Child = layout;

        PointerPressed += (_, e) =>
        {
            if (TopLevel.GetTopLevel(this) is not Window window || !e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
                return;

            window.BeginMoveDrag(e);
            // Помечаем обработанным, чтобы окно не запускало перетаскивание второй раз.
            e.Handled = true;
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleProperty)
            _titleText.Text = change.NewValue as string;
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
