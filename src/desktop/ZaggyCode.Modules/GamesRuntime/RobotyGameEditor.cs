using ZaggyCode.Core.GamesRuntime.Model.GameObjects;

namespace ZaggyCode.Modules.GamesRuntime;

public sealed class RobotyGameEditor : IRobotyGameEditor
{
    private FileStream? _stream;
    public Game OpenEditable(string path)
    {
        if (File.Exists(path))
        {
            _stream = File.Open(path, FileMode.Open);
            var game = new XmlSerializer(typeof(Game)).Deserialize(_stream) as Game;
            if (game is not null)
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
