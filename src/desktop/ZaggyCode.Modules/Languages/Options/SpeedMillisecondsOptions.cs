namespace ZaggyCode.Modules.Languages.Options;

public sealed class SpeedMillisecondsOptions
{
    public required int X1 { get; set; }
    public required int X2 { get; set; }
    public required int X5 { get; set; }
    public required int X10 { get; set; }
    public required int X20 { get; set; }
    
    //Проблема в том, что если отмена делается во время бездействия LanguageRunner то она будет выполнена только 
    //после того, как будет выполнена следующая строка кода, и поэтому все ожидание делиться на чанки
    //программа ждет чанк а затем проверяет токен отмены и ждет еще чанк
    public required int SleepChunk { get; set; }
}
