using ZaggyCode.Core.Data.Enums;

namespace ZaggyCode.Core.Data.Model;

public sealed class ViewSettings : INotifyPropertyChanged
{
    public bool UseSystemTitleBar { get; set => SetField(ref field, value); }
    public SidebarPosition SidebarPosition { get; set => SetField(ref field, value); }
    public int CodeFontSize { get; set => SetField(ref field, value); }
    public int TerminalFontSize { get; set => SetField(ref field, value); }
    public int DescriptionFontSize { get; set => SetField(ref field, value); }
    public required string CodeTheme { get; set => SetField(ref field, value); }

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
