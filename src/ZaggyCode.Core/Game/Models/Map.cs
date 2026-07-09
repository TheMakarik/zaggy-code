namespace ZaggyCode.Core.Game.Models;

[XmlRoot("map")]
public sealed class Map : INotifyPropertyChanged, INotifyCollectionChanged
{
    [XmlArray("points-collection")]
    [XmlArrayItem("point")]
    public required ObservableCollection<Point> Points 
    { 
        get;
        set
        {
            var oldCollection = field;
            if (!SetField(ref field, value))
                return;
            
            #pragma warning disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            
            if(oldCollection is not null)
                oldCollection.CollectionChanged -= OnPointsCollectionChanged;
            
            #pragma warning restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            
            value.CollectionChanged += OnPointsCollectionChanged;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    
    [XmlAttribute("width-points")]
    public int Width { get; set => SetField(ref field, value); }
    
    [XmlAttribute("height-points")]
    public int Height { get; set => SetField(ref field, value); }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private void OnPointsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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