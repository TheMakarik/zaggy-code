using System.Collections.ObjectModel;
using ZaggyCode.Modules.Game;

namespace ZaggyCode.Tests.Game;

public class RobotExecutorTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void MoveUp_WhenNoWalls_MovesRobotAndRaisesPointUpdated()
    {
        // Arrange
        var map = CreateDefaultMap();
        var systemUnderTests = new RobotExecutor(map);
        RobotPointUpdatedEventArgs? updatedArgs = null;
        systemUnderTests.RobotPointUpdated += (_, args) => updatedArgs = args;

        // Act
        systemUnderTests.MoveUp();

        // Assert
        updatedArgs.Should().NotBeNull();
        updatedArgs!.NewX.Should().Be(2);
        updatedArgs.NewY.Should().Be(1);
        systemUnderTests.IsWallFromDown().Should().BeFalse();
    }

    [Fact]
    public void MoveRight_WhenNoWalls_MovesRobotAndRaisesPointUpdated()
    {
        // Arrange
        var map = CreateDefaultMap();
        var systemUnderTests = new RobotExecutor(map);
        RobotPointUpdatedEventArgs? updatedArgs = null;
        systemUnderTests.RobotPointUpdated += (_, args) => updatedArgs = args;

        // Act
        systemUnderTests.MoveRight();

        // Assert
        updatedArgs.Should().NotBeNull();
        updatedArgs!.NewX.Should().Be(3);
        updatedArgs.NewY.Should().Be(2);
    }

    [Fact]
    public void MoveUp_WhenCurrentCellHasTopWall_RaisesRobotDiedWithWall()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 2, 2).WallType = WallType.Top;
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        RobotPointUpdatedEventArgs? updatedArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;
        systemUnderTests.RobotPointUpdated += (_, args) => updatedArgs = args;

        // Act
        systemUnderTests.MoveUp();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.Wall);
        updatedArgs.Should().BeNull();
    }

    [Fact]
    public void MoveUp_WhenTargetCellHasBottomWall_RaisesRobotDiedWithWall()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 2, 1).WallType = WallType.Bottom;
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.MoveUp();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.Wall);
    }

    [Fact]
    public void MoveDown_WhenTargetCellIsFullWall_RaisesRobotDiedWithWall()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 2, 3).WallType = WallType.Full;
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.MoveDown();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.Wall);
    }

    [Fact]
    public void MoveRight_WhenTargetBeyondMapWidth_RaisesRobotDiedWithEndOfTheMap()
    {
        // Arrange
        var map = CreateDefaultMap(spawnX: 3);
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.MoveRight();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.EndOfTheMap);
    }

    [Fact]
    public void MoveRight_WhenPointBeyondMapExists_StillRaisesRobotDiedWithEndOfTheMap()
    {
        // Arrange
        var map = CreateDefaultMap(spawnX: 3);
        map.Points.Add(new RobotGamePoint { X = 4, Y = 2 });
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.MoveRight();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.EndOfTheMap);
    }

    [Fact]
    public void MoveLeft_WhenAtLeftMapEdge_RaisesRobotDiedWithEndOfTheMap()
    {
        // Arrange
        var map = CreateDefaultMap(spawnX: 1);
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.MoveLeft();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.EndOfTheMap);
    }

    [Fact]
    public void MoveUp_WhenAtTopMapEdge_RaisesRobotDiedWithEndOfTheMap()
    {
        // Arrange
        var map = CreateDefaultMap(spawnY: 1);
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.MoveUp();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.EndOfTheMap);
    }

    [Fact]
    public void Move_WhenTargetPointMissingWithinMap_RaisesRobotDiedWithEndOfTheMap()
    {
        // Arrange
        var map = CreateDefaultMap();
        map.Points.Remove(GetPoint(map, 2, 3));
        var systemUnderTests = new RobotExecutor(map);
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.MoveDown();

        // Assert
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.EndOfTheMap);
    }

    [Fact]
    public void IsWallFromUp_WhenCurrentCellHasTopWall_ReturnsTrue()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 2, 2).WallType = WallType.Top;
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsWallFromUp().Should().BeTrue();
        systemUnderTests.IsWallFromDown().Should().BeFalse();
        systemUnderTests.IsWallFromLeft().Should().BeFalse();
        systemUnderTests.IsWallFromRight().Should().BeFalse();
    }

    [Fact]
    public void IsWallFromLeft_WhenTargetCellHasRightWall_ReturnsTrue()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 1, 2).WallType = WallType.Right;
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsWallFromLeft().Should().BeTrue();
    }

    [Fact]
    public void IsWallFromRight_WhenTargetCellIsFullWall_ReturnsTrue()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 3, 2).WallType = WallType.Full;
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsWallFromRight().Should().BeTrue();
    }

    [Fact]
    public void IsWallFromRight_WhenBeyondMapEdge_ReturnsTrue()
    {
        // Arrange
        var map = CreateDefaultMap(spawnX: 3);
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsWallFromRight().Should().BeTrue();
    }

    [Fact]
    public void IsWallFromUp_WhenAtTopMapEdge_ReturnsTrue()
    {
        // Arrange
        var map = CreateDefaultMap(spawnY: 1);
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsWallFromUp().Should().BeTrue();
    }

    [Fact]
    public void IsWallFromLeft_WhenNoWalls_ReturnsFalse()
    {
        // Arrange
        var map = CreateDefaultMap();
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsWallFromUp().Should().BeFalse();
        systemUnderTests.IsWallFromDown().Should().BeFalse();
        systemUnderTests.IsWallFromLeft().Should().BeFalse();
        systemUnderTests.IsWallFromRight().Should().BeFalse();
    }

    [Fact]
    public void FillCell_WhenCellRequiresDraw_RaisesDrawPointWithCurrentPoint()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 2, 2).RequireDraw = true;
        var systemUnderTests = new RobotExecutor(map);
        DrawPointEventArgs? drawArgs = null;
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.DrawPoint += (_, args) => drawArgs = args;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.FillCell();

        // Assert
        drawArgs.Should().NotBeNull();
        drawArgs!.RobotGamePointToDraw.Should().BeSameAs(GetPoint(map, 2, 2));
        deadArgs.Should().BeNull();
    }

    [Fact]
    public void FillCell_WhenCellDoesNotRequireDraw_RaisesRobotDiedWithDrawUnrequiredCell()
    {
        // Arrange
        var map = CreateDefaultMap();
        var systemUnderTests = new RobotExecutor(map);
        DrawPointEventArgs? drawArgs = null;
        RobotDeadEventArgs? deadArgs = null;
        systemUnderTests.DrawPoint += (_, args) => drawArgs = args;
        systemUnderTests.RobotDied += (_, args) => deadArgs = args;

        // Act
        systemUnderTests.FillCell();

        // Assert
        drawArgs.Should().BeNull();
        deadArgs.Should().NotBeNull();
        deadArgs!.DeadType.Should().Be(RobotDeadType.DrawUnrequiredCell);
    }

    [Fact]
    public void IsCellFilled_WhenCurrentCellRequiresDraw_ReturnsTrue()
    {
        // Arrange
        var map = CreateDefaultMap();
        GetPoint(map, 2, 2).RequireDraw = true;
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsCellFilled().Should().BeTrue();
    }

    [Fact]
    public void IsCellFilled_WhenCurrentCellDoesNotRequireDraw_ReturnsFalse()
    {
        // Arrange
        var map = CreateDefaultMap();
        var systemUnderTests = new RobotExecutor(map);

        // Act & Assert
        systemUnderTests.IsCellFilled().Should().BeFalse();
    }

    private static Map CreateDefaultMap(int spawnX = 2, int spawnY = 2, int width = 3, int height = 3)
    {
        ObservableCollection<RobotGamePoint> points = [];
        for (var y = 1; y <= height; y++)
        {
            for (var x = 1; x <= width; x++)
            {
                points.Add(new RobotGamePoint
                {
                    X = x,
                    Y = y,
                    IsSpawn = x == spawnX && y == spawnY
                });
            }
        }

        return new Map { Width = width, Height = height, Points = points };
    }

    private static RobotGamePoint GetPoint(Map map, int x, int y)
    {
        return map.Points.First(point => point.X == x && point.Y == y);
    }
}
