namespace ZaggyCode.Core.Theming.Model;

public sealed class ThemeColorEntry : INotifyPropertyChanged
{
    public ThemeColorEntry(string propertyName, string resourceName, string displayName, string value)
    {
        PropertyName = propertyName;
        ResourceName = resourceName;
        DisplayName = displayName;
        OriginalValue = value;
        _value = value;
    }

    public string PropertyName { get; }
    public string ResourceName { get; }
    public string DisplayName { get; }
    public string OriginalValue { get; private set; }

    private string _value;

    public string Value
    {
        get => _value;
        set
        {
            if (!SetField(ref _value, value))
                return;

            OnPropertyChanged(nameof(IsChanged));
        }
    }

    public bool IsChanged => !string.Equals(Value, OriginalValue, StringComparison.OrdinalIgnoreCase);

    public void MarkSaved()
    {
        OriginalValue = Value;
        OnPropertyChanged(nameof(IsChanged));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
