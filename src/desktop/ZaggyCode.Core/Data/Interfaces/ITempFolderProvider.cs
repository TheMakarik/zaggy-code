namespace ZaggyCode.Core.Data.Interfaces;

public interface ITempFolderProvider
{
    string GetTempPath();

    string GetToCompressPath();

    string GetFromCompressPath();
}
