using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ZaggyCode.Games.Enums;

namespace ZaggyCode.Games.Models;

[XmlRoot("game")]
public sealed class Game : INotifyPropertyChanged
{
    [XmlAttribute("name")]
    public string? Name { get; set => SetField(ref field, value); }

    [XmlIgnore] 
    public string? Path { get; set; }

    [XmlAttribute("description")]
    public string? Description { get; set => SetField(ref field, value); }
    
    [XmlAttribute("author")]
    public string? Author { get; set => SetField(ref field, value); }
    
    [XmlElement("map")]
    public required Map Map { get; set => SetField(ref field, value); }
    
    [XmlAttribute("difficulty")]
    public required Difficulty Difficulty { get; set => SetField(ref field, value); }

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