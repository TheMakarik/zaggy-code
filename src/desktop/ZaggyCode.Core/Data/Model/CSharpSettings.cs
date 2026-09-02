namespace ZaggyCode.Core.Data.Model;

public sealed class CSharpSettings : INotifyPropertyChanged
{
    public bool UseTopLevelStatements
    {
        get => field;
        set => SetField(ref field, value);
    }

    public bool EnableImplicitUsings
    {
        get => field;
        set => SetField(ref field, value);
    }

    public bool BlockIo
    {
        get => field;
        set => SetField(ref field, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
