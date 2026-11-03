namespace ZaggyCode.Core.RobotGameCore.Model;

public sealed class RobotGameState : INotifyPropertyChanged
{
    public ExecutionSpeed Speed { get; set => SetField(ref field, value); }

    public RobotMap? Map { get; set => SetField(ref field, value); }

    public Language Language { get; set => SetField(ref field, value); }

    public bool IsRunning { get; set => SetField(ref field, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
