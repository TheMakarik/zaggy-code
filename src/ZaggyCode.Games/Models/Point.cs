using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ZaggyCode.Games.Models;

public sealed class Point : INotifyPropertyChanged
{
    [XmlAttribute("x")]
    public int X { get; set => SetField(ref field, value); }
    
    [XmlAttribute("y")]
    public int Y { get; set => SetField(ref field, value); }
    
    [XmlAttribute("wall")]
    public bool IsWall { get; set => SetField(ref field, value); }
    
    [XmlAttribute("want-draw")]
    public bool RequireDraw { get; set => SetField(ref field, value); }
    
    [XmlAttribute("spawn")]
    public bool IsSpawn { get; set => SetField(ref field, value); }
    
    [XmlAttribute("coin")]
    public bool HasCoin { get; set => SetField(ref field, value); }

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