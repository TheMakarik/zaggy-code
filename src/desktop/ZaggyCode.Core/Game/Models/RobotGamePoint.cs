namespace ZaggyCode.Core.Game.Models;

[XmlRoot("game-point")]
public class RobotGamePoint : INotifyPropertyChanged
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

    [XmlAttribute("goal")]
    [DefaultValue(false)]
    public bool IsGoal { get; set => SetField(ref field, value); }
    
    [XmlAttribute("customization-props:background-hex")]
    [DefaultValue(null)]
    public string? CustomBackgroundHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("customization-props:border-hex")]
    [DefaultValue(null)]
    public string? CustomBorderHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("customization-props:wall-on-point-hex")]
    [DefaultValue(null)]
    public string? CustomWallOnPointHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("customization-props:drew-point-hex")]
    [DefaultValue(null)]
    public string? CustomDrewPointHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("customization-props:wall-opacity")]
    [DefaultValue(null)]
    public int? CustomWallOpacity { get; set => SetField(ref field, value); }

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

    public static explicit operator System.Drawing.Point(RobotGamePoint robotGamePoint)
    {
        return new System.Drawing.Point(robotGamePoint.X, robotGamePoint.Y);
    }
}
