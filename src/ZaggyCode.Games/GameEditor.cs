using System.Xml.Serialization;
using ZaggyCode.Games.Interfaces;
using ZaggyCode.Games.Models;

namespace ZaggyCode.Games;

public sealed class GameEditor : IGameEditor
{
    private FileStream _stream;
    public Game OpenEditable(string path)
    {
        if (File.Exists(path))
        {
            _stream = File.Open(path, FileMode.Open);
            var game = new XmlSerializer(typeof(Game)).Deserialize(_stream) as Game;
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