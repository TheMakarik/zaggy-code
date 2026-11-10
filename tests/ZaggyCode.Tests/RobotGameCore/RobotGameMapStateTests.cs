using System.Drawing;
using ZaggyCode.Core.RobotGameCore.Model;

namespace ZaggyCode.Tests.RobotGameCore;

public class RobotGameMapStateTests
{
    [Fact]
    public void MoveTo_WhenCalled_ChangesPosition()
    {
        // Arrange
        var systemUnderTests = new RobotGameMapState(new Point(1, 2));
        var expected = new Point(3, 4);

        // Act
        systemUnderTests.MoveTo(expected);

        // Assert
        systemUnderTests.Position.Should().Be(expected);
    }

    [Fact]
    public void MarkDrawn_WhenPointIsNew_AddsPointAndReturnsTrue()
    {
        // Arrange
        var systemUnderTests = new RobotGameMapState(new Point(0, 0));
        var point = new Point(1, 2);

        // Act
        var added = systemUnderTests.MarkDrawn(point);

        // Assert
        added.Should().BeTrue();
        systemUnderTests.IsDrawn(point).Should().BeTrue();
        systemUnderTests.DrawnPoints.Should().ContainSingle().Which.Should().Be(point);
    }

    [Fact]
    public void MarkDrawn_WhenPointAlreadyDrawn_ReturnsFalseAndKeepsSinglePoint()
    {
        // Arrange
        var systemUnderTests = new RobotGameMapState(new Point(0, 0));
        var point = new Point(1, 2);
        systemUnderTests.MarkDrawn(point);

        // Act
        var added = systemUnderTests.MarkDrawn(point);

        // Assert
        added.Should().BeFalse();
        systemUnderTests.DrawnPoints.Should().ContainSingle();
    }

    [Fact]
    public void CollectCoin_WhenCoinIsNew_AddsCoinAndReturnsTrue()
    {
        // Arrange
        var systemUnderTests = new RobotGameMapState(new Point(0, 0));
        var point = new Point(2, 3);
        var expectedCoin = point;

        // Act
        var collected = systemUnderTests.CollectCoin(point);

        // Assert
        collected.Should().BeTrue();
        systemUnderTests.IsCoinCollected(point).Should().BeTrue();
        systemUnderTests.CollectedCoins.Should().ContainSingle().Which.Should().Be(expectedCoin);
    }

    [Fact]
    public void CollectCoin_WhenCoinAlreadyCollected_ReturnsFalseAndKeepsSingleCoin()
    {
        // Arrange
        var systemUnderTests = new RobotGameMapState(new Point(0, 0));
        var point = new Point(2, 3);
        systemUnderTests.CollectCoin(point);

        // Act
        var collected = systemUnderTests.CollectCoin(point);

        // Assert
        collected.Should().BeFalse();
        systemUnderTests.CollectedCoins.Should().ContainSingle();
    }

    [Fact]
    public void Reset_WhenCalled_ClearsCollectedAndDrawnAndRestoresPosition()
    {
        // Arrange
        var spawn = new Point(0, 0);
        var systemUnderTests = new RobotGameMapState(spawn);
        systemUnderTests.MoveTo(new Point(2, 2));
        systemUnderTests.MarkDrawn(new Point(1, 1));
        systemUnderTests.CollectCoin(new Point(2, 1));

        // Act
        systemUnderTests.Reset(spawn);

        // Assert
        systemUnderTests.Position.Should().Be(spawn);
        systemUnderTests.DrawnPoints.Should().BeEmpty();
        systemUnderTests.CollectedCoins.Should().BeEmpty();
    }
}
