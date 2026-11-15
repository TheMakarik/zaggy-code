namespace ZaggyCode.Core.Theming.Model;

public sealed class ThemeColorEntry(string propertyName, string resourceName, string displayName, string value)
    : INotifyPropertyChanged
{
    public string PropertyName { get; } = propertyName;
    public string ResourceName { get; } = resourceName;
    public string DisplayName { get; } = displayName;
    public string OriginalValue { get; private set; } = value;

    private string _value = value;

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
