

using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Modules.RobotGameCore;

public sealed class RobotyGameEditor
{
    private FileStream? _stream;
    public RobotGame OpenEditable(string path)
    {
        if (File.Exists(path))
        {
            _stream = File.Open(path, FileMode.Open);
            var game = new XmlSerializer(typeof(RobotGame)).Deserialize(_stream) as RobotGame;
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
