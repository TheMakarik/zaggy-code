using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ZaggyCode.Games.Enums;

namespace ZaggyCode.Games.Models;
[XmlRoot("game")]
public sealed class Game : INotifyPropertyChanged, INotifyCollectionChanged
{
    [XmlAttribute("name")]
    public string? Name { get; set => SetField(ref field, value); }

    [XmlIgnore] 
    public string? Path { get; set; }

    [XmlAttribute("description")]
    public string? Description { get; set => SetField(ref field, value); }
    
    [XmlAttribute("author")]
    public string? Author { get; set => SetField(ref field, value); }
    
    [XmlElement("maps")]
    public required ObservableCollection<Map> Maps 
    { 
        get;
        set
        {
            var oldCollection = field;
            if (!SetField(ref field, value))
                return;
            
            oldCollection.CollectionChanged -= OnMapsCollectionChanged;
            value.CollectionChanged += OnMapsCollectionChanged;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    
    [XmlAttribute("difficulty")]
    public required Difficulty Difficulty { get; set => SetField(ref field, value); }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private void OnMapsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnCollectionChanged(e);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}