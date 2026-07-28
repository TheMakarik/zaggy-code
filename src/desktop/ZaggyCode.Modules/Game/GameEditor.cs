using ZaggyCode.Core.Game.Interfaces;

namespace ZaggyCode.Modules.Game;

public sealed class GameEditor : IGameEditor
{
    private FileStream _stream;
    public GameModel OpenEditable(string path)
    {
        if (File.Exists(path))
        {
            _stream = File.Open(path, FileMode.Open);
            var game = new XmlSerializer(typeof(GameModel)).Deserialize(_stream) as GameModel;
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
