namespace ZaggyCode.Core.Game.Events;

public class OverrodeGameComponentEventArgs : System.EventArgs
{
    public required IReadOnlyCollection<Point> PointsToUpdate { get; set; } = [];
    public string? NewName { get; set; }
    public string? NewDescription { get; set; }
    
    //При обновлении высоты или ширины нужно дорисовать новые Point, или СКРЫТЬ (не удалять) уже существующие
    public int? NewWidth { get; set; }
    public int? NewHeight { get; set; }
}