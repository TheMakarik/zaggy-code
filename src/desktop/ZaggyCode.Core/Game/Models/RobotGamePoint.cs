namespace ZaggyCode.Core.Game.Models;

[XmlRoot("game-point")]
public class RobotGamePoint : INotifyPropertyChanged
{
    [XmlAttribute("x")]
    public int X { get; set => SetField(ref field, value); }

    [XmlAttribute("y")]
    public int Y { get; set => SetField(ref field, value); }
    
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

    #region Core

    [XmlAttribute("CORE.hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomCoreBackgroundHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE.opacity", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomCoreOpacity { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE.drew-point-background-hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomCoreDrewPointHex { get; set => SetField(ref field, value); }

    #endregion

    #region Core border

    [XmlAttribute("CORE_BORDER.hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomBorderHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.opacity", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderOpacity { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.radius-left-top", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomBorderRadiusLeftTop { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.radius-left-bottom", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomBorderRadiusLeftBottom { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.radius-right-top", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomBorderRadiusRightTop { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.radius-right-bottom", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomBorderRadiusRightBottom { get; set => SetField(ref field, value); }

    #endregion

    #region Wall

    [XmlAttribute("WALL.type", Namespace = "objects")]
    public WallType WallType { get; set => SetField(ref field, value); }
    
    [XmlAttribute("WALL.hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomWallOnPointHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("WALL.border-hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomWallBorderOnPointHex { get; set => SetField(ref field, value); }
    
    
    [XmlAttribute("WALL.opacity", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomWallOpacity { get; set => SetField(ref field, value); }

    #endregion
    
    

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
