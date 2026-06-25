namespace ZaggyCode.Data.Options;

public sealed class StorageOptions
{
    public required string GameCodeDataPath { get; set ; }
    public required string DataFilePath { get; set; }
    public required int WaitUserDataUpdateSeconds { get; set; }
}