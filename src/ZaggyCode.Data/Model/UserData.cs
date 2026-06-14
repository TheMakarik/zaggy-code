using System.ComponentModel;
using System.Runtime.CompilerServices;
using ZaggyCode.Languages.Enums;

namespace ZaggyCode.Data.Model;

public sealed class UserData : INotifyPropertyChanged
{
    public bool EnableCodeHighlighting { get; set => SetField(ref field, value); }
    public bool ShowCodeLineNumbers { get; set => SetField(ref field, value); }
    public int CodeFontSize { get; set => SetField(ref field, value); }
    public required string CodeTheme { get; set => SetField(ref field, value); }
    public required Language LastLanguage { get; set => SetField(ref field, value); }
    public required string? LastGamePath { get; set => SetField(ref field, value); }

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