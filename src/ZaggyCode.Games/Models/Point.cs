using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ZaggyCode.Games.Enums;

namespace ZaggyCode.Games.Models;

[XmlRoot("point")]
public class Point : INotifyPropertyChanged
{
    [XmlAttribute("x")]
    public int X { get; set => SetField(ref field, value); }
    
    [XmlAttribute("y")]
    public int Y { get; set => SetField(ref field, value); }
    
    [XmlAttribute("wall-type")]
    public WallType WallType { get; set => SetField(ref field, value); }
    
    [XmlAttribute("want-draw")]
    [DefaultValue(false)]
    public bool RequireDraw { get; set => SetField(ref field, value); }
    
    [XmlAttribute("spawn")]
    [DefaultValue(false)]
    public bool IsSpawn { get; set => SetField(ref field, value); }
    
    [XmlAttribute("coin")]
    [DefaultValue(false)]
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
    
    public static explicit operator System.Drawing.Point(Point point)
    {
        return new System.Drawing.Point(point.X, point.Y);
    }
}