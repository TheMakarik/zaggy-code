namespace ZaggyCode.Core.Data.Model;

public sealed class ThemeSettings : INotifyPropertyChanged
{
    public string CurrentTheme { get; set => SetField(ref field, value); } = "Primus";

    public ThemeCreationBehavior OnCreateBehavior { get; set => SetField(ref field, value); }

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
