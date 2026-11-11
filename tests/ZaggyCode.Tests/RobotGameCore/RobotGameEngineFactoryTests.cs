using System.Collections.ObjectModel;
using ZaggyCode.Core.BackgroundManagement;
using ZaggyCode.Core.BackgroundManagement.Dto;
using ZaggyCode.Core.BackgroundManagement.Model;
using ZaggyCode.Core.LanguageRuntime.Factories;
using ZaggyCode.Core.Proxies.Interfaces;
using ZaggyCode.Core.RobotGameCore.Dto;
using ZaggyCode.Core.RobotGameCore.Factories;
using ZaggyCode.Modules.RobotGameCore;

namespace ZaggyCode.Tests.RobotGameCore;

public class RobotGameEngineFactoryTests
{
    [Fact]
    public async Task CreateAsync_UsesMapFromDto()
    {
        // Arrange
        var selectedMap = CreateMap(5, spawnX: 4, spawnY: 4);
        var dto = new RobotGameEngineInitDto
        {
            Input = TextReader.Null,
            Output = TextWriter.Null,
            Language = Language.Python,
            Speed = ExecutionSpeed.X1,
            Map = selectedMap
        };

        RobotMap? executorMap = null;
        var executorFactory = A.Fake<IRobotExecutorFactory>();
        A.CallTo(() => executorFactory.Create(A<RobotMap>._))
            .Invokes((RobotMap map) => executorMap = map)
            .Returns(A.Fake<IRobotExecutor>());

        var languageRunnerFactory = A.Fake<ILanguageRunnerFactory>();
        A.CallTo(() => languageRunnerFactory.Create(A<LanguageRunnerDto>._))
            .Returns(A.Fake<ILanguageRunner>());

        var backgroundTaskHelper = A.Fake<IBackgroundTaskHelper>();
        A.CallTo(() => backgroundTaskHelper.StartBackground(A<BackgroundTaskStartDto>._))
            .Invokes((BackgroundTaskStartDto startDto) =>
                startDto.DelegateToStart(new Progress<BackgroundProgressMove>()));

        var systemUnderTests = new RobotGameEngineFactory(
            executorFactory,
            languageRunnerFactory,
            A.Fake<IRobotController>(),
            A.Fake<IRobotGameMapProxy>(),
            A.Fake<IRobotGameEditorProxy>(),
            backgroundTaskHelper,
            Options.Create(CreateSpeedOptions()),
            A.Fake<ILogger<RobotGameEngine>>());

        // Act
        var engine = await systemUnderTests.CreateAsync(dto);

        // Assert
        engine.State.Map.Should().BeSameAs(selectedMap);
        executorMap.Should().BeSameAs(selectedMap);
    }

    private static SpeedMillisecondsOptions CreateSpeedOptions() => new()
    {
        X1 = 1000,
        X2 = 500,
        X5 = 200,
        X10 = 100,
        X20 = 50,
        SleepChunk = 50
    };

    private static RobotMap CreateMap(int size, int spawnX, int spawnY)
    {
        ObservableCollection<RobotGamePoint> points = [];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            points.Add(new RobotGamePoint { X = x, Y = y, IsSpawn = x == spawnX && y == spawnY });

        return new RobotMap
        {
            Width = size,
            Height = size,
            Points = points
        };
    }
}
