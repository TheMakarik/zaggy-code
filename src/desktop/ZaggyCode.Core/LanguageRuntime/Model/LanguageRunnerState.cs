using System.Threading.Channels;
using ZaggyCode.Core.LanguageRuntime.Enums;

namespace ZaggyCode.Core.LanguageRuntime.Model;

//Состояние раннера языка, то, что может поменяться в любой момент реализует NotifyPropertyChanged евент
//изменения могут произойти в любой момент, будь то во время работы кода, или до запуска
//раннер обязан принять эти изменения
public sealed class LanguageRunnerState(ExecutionSpeed speed) : INotifyPropertyChanged
{
    public ExecutionSpeed Speed { get; init; } = speed;
    public required ChannelWriter<ComputedLineOfCode> ExecutedLines { get; init; }

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