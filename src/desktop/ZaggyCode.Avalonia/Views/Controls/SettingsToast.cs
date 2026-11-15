using Avalonia.Threading;
using Material.Icons;
using Material.Icons.Avalonia;

namespace ZaggyCode.Avalonia.Views.Controls;

//#:NO_AI
public sealed class SettingsToast : Border
{
    private const double FadeTickSeconds = 0.05;
    private const double DefaultDisplaySeconds = 1.5;
    private const double DefaultFadeOutSeconds = 0.5;

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<SettingsToast, string?>(nameof(Message));

    public static readonly StyledProperty<IReadOnlyCollection<ToastAction>> ActionsProperty =
        AvaloniaProperty.Register<SettingsToast, IReadOnlyCollection<ToastAction>>(
            nameof(Actions),
            []);

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public IReadOnlyCollection<ToastAction> Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public event EventHandler? Dismissed;

    private readonly TextBlock _messageText;
    private readonly StackPanel _actionsPanel;
    private readonly DispatcherTimer _displayTimer;
    private readonly DispatcherTimer _fadeOutTimer;
    private TimeSpan _displayInterval = TimeSpan.Zero;
    private TimeSpan _fadeOutDuration = TimeSpan.Zero;
    private int _fadeOutTicksLeft;
    private int _fadeOutTotalTicks;

    public SettingsToast()
    {
        Classes.Add("settings-toast");
        IsVisible = false;
        Opacity = 0;
        HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center;
        VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Bottom;
        Margin = new Thickness(0, 0, 0, 20);

        _messageText = new TextBlock { VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center };

        _actionsPanel = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center
        };

        var closeButton = new Button
        {
            Classes = { "icon-button" },
            Content = new MaterialIcon { Kind = MaterialIconKind.Close },
            Margin = new Thickness(5, 0, 0, 0)
        };
        closeButton.Click += (_, _) => Hide();

        var layout = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto") };
        Grid.SetColumn(_messageText, 0);
        Grid.SetColumn(_actionsPanel, 1);
        Grid.SetColumn(closeButton, 2);
        layout.Children.Add(_messageText);
        layout.Children.Add(_actionsPanel);
        layout.Children.Add(closeButton);

        Child = layout;

        _displayTimer = new DispatcherTimer();
        _displayTimer.Tick += (_, _) =>
        {
            _displayTimer.Stop();
            StartFadeOut();
        };

        _fadeOutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(FadeTickSeconds) };
        _fadeOutTimer.Tick += (_, _) => FadeOutTick();
    }

    public void Show(string message, params ToastAction[] actions)
    {
        ApplyOptions();

        Message = message;
        Actions = actions;
        Opacity = 1;
        IsVisible = true;

        _displayTimer.Stop();
        _fadeOutTimer.Stop();
        _displayTimer.Interval = _displayInterval;
        _displayTimer.Start();
    }

    public void Hide()
    {
        _displayTimer.Stop();
        _fadeOutTimer.Stop();
        Opacity = 0;
        IsVisible = false;
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MessageProperty)
            _messageText.Text = change.NewValue as string;

        if (change.Property == ActionsProperty)
            RebuildActions();
    }

    private void RebuildActions()
    {
        _actionsPanel.Children.Clear();

        if (Actions.Count == 0)
        {
            _actionsPanel.IsVisible = false;
            return;
        }

        _actionsPanel.IsVisible = true;

        foreach (var action in Actions)
            _actionsPanel.Children.Add(CreateActionButton(action));
    }

    private static Button CreateActionButton(ToastAction action)
    {
        var content = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 6
        };

        var iconKind = GetActionIcon(action.Kind);
        if (iconKind is { } kind)
            content.Children.Add(new MaterialIcon { Kind = kind });
        else
            content.Children.Add(new TextBlock { Text = action.Text, VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center });

        var button = new Button { Content = content, Margin = new Thickness(9, 0, 0, 0) };
        button.Classes.Add("toast-action");
        ToolTip.SetTip(button, action.Text);
        button.Click += (_, _) => action.Click?.Invoke();
        return button;
    }

    private static MaterialIconKind? GetActionIcon(ToastActionKind kind)
        => kind switch
        {
            ToastActionKind.Revert => MaterialIconKind.Undo,
            ToastActionKind.Save => MaterialIconKind.ContentSave,
            _ => null
        };

    private void ApplyOptions()
    {
        var options = App.Services?.GetService<IOptions<PopupOptions>>()?.Value;
        var displaySeconds = options?.PopupDisplaySeconds ?? DefaultDisplaySeconds;
        var fadeOutSeconds = options?.PopupFadeOutSeconds ?? DefaultFadeOutSeconds;

        // Плашка подтверждает действие, а не требует прочтения: держим её
        // на 30% меньше базового времени, чтобы она не мешала работе.
        _displayInterval = TimeSpan.FromSeconds(displaySeconds * 0.7);
        _fadeOutDuration = TimeSpan.FromSeconds(fadeOutSeconds);
    }

    private void StartFadeOut()
    {
        _fadeOutTicksLeft = Math.Max(1, (int)(_fadeOutDuration.TotalSeconds / FadeTickSeconds));
        _fadeOutTotalTicks = _fadeOutTicksLeft;
        _fadeOutTimer.Start();
    }

    private void FadeOutTick()
    {
        _fadeOutTicksLeft--;
        Opacity = Math.Max(0, (double)_fadeOutTicksLeft / _fadeOutTotalTicks);

        if (_fadeOutTicksLeft > 0)
            return;

        Hide();
    }
}
