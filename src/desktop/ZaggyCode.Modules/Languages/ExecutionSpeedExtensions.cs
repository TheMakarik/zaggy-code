namespace ZaggyCode.Modules.Languages;

public static class ExecutionSpeedExtensions
{
    public static int GetActual(this ExecutionSpeed speed, SpeedMillisecondsOptions millisecondsOptions)
    {
        return speed switch
        {
            ExecutionSpeed.X1 => millisecondsOptions.X1,
            ExecutionSpeed.X2 => millisecondsOptions.X2,
            ExecutionSpeed.X5 => millisecondsOptions.X5,
            ExecutionSpeed.X10 => millisecondsOptions.X10,
            ExecutionSpeed.X20 => millisecondsOptions.X20,
            _ => throw new ArgumentOutOfRangeException(nameof(speed), speed, null)
        };
    }
}
