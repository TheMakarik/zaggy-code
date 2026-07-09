namespace ZaggyCode.Core.Game;

public sealed class GameEditor : IGameEditor
{
    private FileStream _stream;
    public Models.Game OpenEditable(string path)
    {
        if (File.Exists(path))
        {
            _stream = File.Open(path, FileMode.Open);
            var game = new XmlSerializer(typeof(Models.Game)).Deserialize(_stream) as Models.Game;
            game.Path = path;
        }
        
        throw new NotImplementedException();
    }

    
    public void Dispose()
    {
        // TODO release managed resources here
    }
    
    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}