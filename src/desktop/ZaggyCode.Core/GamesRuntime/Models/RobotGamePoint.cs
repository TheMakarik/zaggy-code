namespace ZaggyCode.Core.GamesRuntime.Models;

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

    #region Goal
    
    [XmlAttribute("GOAL.color1-hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomGoalFirstColorHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("GOAL.color2-hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomGoalSecondColorHex { get; set => SetField(ref field, value); }

    #endregion

    #region Core

    [XmlAttribute("CORE.hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomCoreBackgroundHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE.opacity", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomCoreOpacity { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE.drew-point-background-hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomCoreDrewPointHex { get; set => SetField(ref field, value); }

    [XmlAttribute("CORE.drew-point-opacity", Namespace = "objects")]
    [DefaultValue(null)]
    public double? RequireDrawCustomOpacity { get; set => SetField(ref field, value); }

    #endregion

    #region Require draw mark

    [XmlAttribute("MARK.width", Namespace = "objects")]
    [DefaultValue(null)]
    public double? RequireDrawMarkWidth { get; set => SetField(ref field, value); }

    [XmlAttribute("MARK.height", Namespace = "objects")]
    [DefaultValue(null)]
    public double? RequireDrawMarkHeight { get; set => SetField(ref field, value); }

    [XmlAttribute("MARK.thickness", Namespace = "objects")]
    [DefaultValue(null)]
    public double? RequireDrawMarkThickness { get; set => SetField(ref field, value); }

    [XmlAttribute("MARK.on-goal-bold", Namespace = "objects")]
    [DefaultValue(false)]
    public bool MakeRequireDrawOnGoalBold { get; set => SetField(ref field, value); }

    #endregion

    #region Core border

    [XmlAttribute("CORE_BORDER.hex", Namespace = "objects")]
    [DefaultValue(null)]
    public string? CustomBorderHex { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.is_visible", Namespace = "objects")]
    [DefaultValue(true)]
    public bool CustomBorderVisible { get; set => SetField(ref field, value); } = true;
    
    [XmlAttribute("CORE_BORDER.radius-left-top", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderRadiusLeftTop { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.radius-left-bottom", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderRadiusLeftBottom { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.radius-right-top", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderRadiusRightTop { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.radius-right-bottom", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderRadiusRightBottom { get; set => SetField(ref field, value); }

    
    [XmlAttribute("CORE_BORDER.margin-top", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderMarginTop { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.margin-bottom", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderMarginBottom { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.margin-right", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderMarginRight { get; set => SetField(ref field, value); }
    
    [XmlAttribute("CORE_BORDER.margin-left", Namespace = "objects")]
    [DefaultValue(null)]
    public int? CustomBorderMarginLeft { get; set => SetField(ref field, value); }

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
