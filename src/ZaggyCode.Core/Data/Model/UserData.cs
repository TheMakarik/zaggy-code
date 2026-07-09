namespace ZaggyCode.Core.Data.Model;

public sealed class UserData : INotifyPropertyChanged
{
    public bool EnableCodeHighlighting { get; set => SetField(ref field, value); }
    public bool ShowCodeLineNumbers { get; set => SetField(ref field, value); }
    public int CodeFontSize { get; set => SetField(ref field, value); }
    public required string CodeTheme { get; set => SetField(ref field, value); }
    public required Language LastLanguage { get; set => SetField(ref field, value); }
    public required string? LastGamePath { get; set => SetField(ref field, value); }
    public required ExecutionSpeed LastSpeed { get; set => SetField(ref field, value); }
    public required int TerminalFontSize { get; set => SetField(ref field, value); }
    public required LuaData LuaData { get; set; }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public UserData()
    {
        var properties = GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(this);
            if (value is INotifyPropertyChanged iNotifyPropertyChanged)
                iNotifyPropertyChanged.PropertyChanged += (s, e) =>
                    OnPropertyChanged(s?.GetType().Name ?? string.Empty + e.PropertyName); ;
        }
    }
    

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