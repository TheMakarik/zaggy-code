namespace ZaggyCode.Core.Data.Model;

public sealed class PythonSettings : INotifyPropertyChanged
{
    public bool UseEntryFunction
    {
        get => field;
        set => SetField(ref field, value);
    }

    public string EntryFunctionName
    {
        get => field;
        set => SetField(ref field, value);
    }

    public bool SupressIo
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