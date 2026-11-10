using System.Collections.ObjectModel;
using System.Drawing;
using ZaggyCode.Core.RobotGameCore.Model;
using ZaggyCode.Modules.RobotGameCore;

namespace ZaggyCode.Tests.RobotGameCore;

public class RobotExecutorTests
{
    [Fact]
    public void Move_WhenNoWalls_MovesRobotAndRaisesSingleStateUpdate()
    {
        // Arrange
        var map = CreateMap();
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);

        // Act
        systemUnderTests.Move(RobotDirection.Up);

        // Assert
        updates.Should().ContainSingle();
        var action = updates[0].Actions.Should().ContainSingle().Subject;
        action.Direction.Should().Be(RobotDirection.Up);
        action.Position.Should().Be(new Point(2, 1));
        action.DeadType.Should().BeNull();
        systemUnderTests.State.Position.Should().Be(new Point(2, 1));
    }

    [Fact]
    public void Move_WhenTargetHasCoin_CollectsCoinAndSendsItInUpdate()
    {
        // Arrange
        var map = CreateMap();
        GetPoint(map, 2, 1).HasCoin = true;
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);

        // Act
        systemUnderTests.Move(RobotDirection.Up);

        // Assert
        var expectedCoin = new Point(2, 1);
        var action = updates.Should().ContainSingle().Subject.Actions.Should().ContainSingle().Subject;
        action.CollectedCoin.Should().Be(expectedCoin);
        systemUnderTests.State.IsCoinCollected(new Point(2, 1)).Should().BeTrue();
        systemUnderTests.State.CollectedCoins.Should().ContainSingle().Which.Should().Be(expectedCoin);
    }

    [Fact]
    public void Move_WhenCoinWasAlreadyCollected_DoesNotSendCoinAgain()
    {
        // Arrange
        var map = CreateMap();
        GetPoint(map, 2, 1).HasCoin = true;
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);
        systemUnderTests.Move(RobotDirection.Up);
        systemUnderTests.Move(RobotDirection.Down);

        // Act
        systemUnderTests.Move(RobotDirection.Up);

        // Assert
        updates.Should().HaveCount(3);
        var action = updates[2].Actions.Should().ContainSingle().Subject;
        action.CollectedCoin.Should().BeNull();
        systemUnderTests.State.CollectedCoins.Should().ContainSingle();
    }

    [Fact]
    public void FillCell_WhenRequireDraw_MarksPointDrawnAndSendsDrawnPoint()
    {
        // Arrange
        var map = CreateMap();
        GetPoint(map, 2, 2).RequireDraw = true;
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);

        // Act
        systemUnderTests.FillCell();

        // Assert
        var action = updates.Should().ContainSingle().Subject.Actions.Should().ContainSingle().Subject;
        action.DrawnPoint.Should().Be(new Point(2, 2));
        action.DeadType.Should().BeNull();
        systemUnderTests.State.IsDrawn(new Point(2, 2)).Should().BeTrue();
        systemUnderTests.IsCellFilled().Should().BeTrue();
    }

    [Fact]
    public void FillCell_WhenPointDoesNotRequireDraw_SendsDrawUnrequiredCellDeath()
    {
        // Arrange
        var map = CreateMap();
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);

        // Act
        systemUnderTests.FillCell();

        // Assert
        var action = updates.Should().ContainSingle().Subject.Actions.Should().ContainSingle().Subject;
        action.DeadType.Should().Be(RobotDeadType.DrawUnrequiredCell);
        action.DrawnPoint.Should().BeNull();
        systemUnderTests.State.IsDrawn(new Point(2, 2)).Should().BeFalse();
    }

    [Fact]
    public void FillCell_WhenPointAlreadyDrawn_DoesNotRaiseUpdate()
    {
        // Arrange
        var map = CreateMap();
        GetPoint(map, 2, 2).RequireDraw = true;
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);
        systemUnderTests.FillCell();

        // Act
        systemUnderTests.FillCell();

        // Assert
        updates.Should().ContainSingle();
    }

    [Fact]
    public void Move_WhenCurrentPointHasTopWall_SendsWallDeathAndDoesNotMove()
    {
        // Arrange
        var map = CreateMap();
        GetPoint(map, 2, 2).RobotWallType = RobotWallType.Top;
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);

        // Act
        systemUnderTests.Move(RobotDirection.Up);

        // Assert
        var action = updates.Should().ContainSingle().Subject.Actions.Should().ContainSingle().Subject;
        action.DeadType.Should().Be(RobotDeadType.Wall);
        action.Direction.Should().Be(RobotDirection.Up);
        systemUnderTests.State.Position.Should().Be(new Point(2, 2));
    }

    [Fact]
    public void Move_WhenTargetIsBeyondMap_SendsEndOfTheMapDeath()
    {
        // Arrange
        var map = CreateMap(spawnX: 3);
        var systemUnderTests = new RobotExecutor(map);
        var updates = new List<RobotStateUpdate>();
        systemUnderTests.StateUpdated += (_, update) => updates.Add(update);

        // Act
        systemUnderTests.Move(RobotDirection.Right);

        // Assert
        var action = updates.Should().ContainSingle().Subject.Actions.Should().ContainSingle().Subject;
        action.DeadType.Should().Be(RobotDeadType.EndOfTheMap);
        systemUnderTests.State.Position.Should().Be(new Point(3, 2));
    }

    [Fact]
    public void CheckWall_WhenTargetIsBeyondMap_ReturnsTrue()
    {
        // Arrange
        var map = CreateMap(spawnX: 3);
        var systemUnderTests = new RobotExecutor(map);

        // Act
        var hasWall = systemUnderTests.CheckWall(RobotDirection.Right);

        // Assert
        hasWall.Should().BeTrue();
    }

    [Fact]
    public void CheckWall_WhenTargetPointHasTopWall_ReturnsTrue()
    {
        // Arrange
        var map = CreateMap();
        GetPoint(map, 2, 3).RobotWallType = RobotWallType.Top;
        var systemUnderTests = new RobotExecutor(map);

        // Act
        var hasWall = systemUnderTests.CheckWall(RobotDirection.Down);

        // Assert
        hasWall.Should().BeTrue();
    }

    [Fact]
    public void CheckWall_WhenNoWalls_ReturnsFalse()
    {
        // Arrange
        var map = CreateMap();
        var systemUnderTests = new RobotExecutor(map);

        // Act
        var hasWall = systemUnderTests.CheckWall(RobotDirection.Left);

        // Assert
        hasWall.Should().BeFalse();
    }

    private static RobotMap CreateMap(int spawnX = 2, int spawnY = 2)
    {
        const int width = 4;
        const int height = 4;

        ObservableCollection<RobotGamePoint> points = [];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                points.Add(new RobotGamePoint { X = x, Y = y });
        }

        GetPoint(points, spawnX, spawnY).IsSpawn = true;

        return new RobotMap
        {
            Width = width,
            Height = height,
            Points = points
        };
    }

    private static RobotGamePoint GetPoint(RobotMap map, int x, int y)
        => GetPoint(map.Points, x, y);

    private static RobotGamePoint GetPoint(IEnumerable<RobotGamePoint> points, int x, int y)
        => points.First(point => point.X == x && point.Y == y);
}
