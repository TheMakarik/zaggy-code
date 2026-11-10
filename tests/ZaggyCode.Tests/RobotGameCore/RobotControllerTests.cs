using ZaggyCode.Core.RobotGameCore.Interfaces;
using ZaggyCode.Modules.RobotGameCore;

namespace ZaggyCode.Tests.RobotGameCore;

public class RobotControllerTests
{
    [Fact]
    public void MoveCommands_DelegateToExecutorMove()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var systemUnderTests = new RobotController(executor);

        // Act
        systemUnderTests.MoveUp();
        systemUnderTests.MoveRight();
        systemUnderTests.MoveDown();
        systemUnderTests.MoveLeft();

        // Assert
        A.CallTo(() => executor.Move(RobotDirection.Up)).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.Move(RobotDirection.Right)).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.Move(RobotDirection.Down)).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.Move(RobotDirection.Left)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void WallChecks_DelegateToExecutorCheckWall()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        A.CallTo(() => executor.CheckWall(RobotDirection.Up)).Returns(true);
        var systemUnderTests = new RobotController(executor);

        // Act
        var hasWallFromUp = systemUnderTests.IsWallFromUp();
        var hasWallFromRight = systemUnderTests.IsWallFromRight();
        var hasWallFromDown = systemUnderTests.IsWallFromDown();
        var hasWallFromLeft = systemUnderTests.IsWallFromLeft();

        // Assert
        hasWallFromUp.Should().BeTrue();
        hasWallFromRight.Should().BeFalse();
        hasWallFromDown.Should().BeFalse();
        hasWallFromLeft.Should().BeFalse();
        A.CallTo(() => executor.CheckWall(RobotDirection.Up)).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.CheckWall(RobotDirection.Right)).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.CheckWall(RobotDirection.Down)).MustHaveHappenedOnceExactly();
        A.CallTo(() => executor.CheckWall(RobotDirection.Left)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void FillCell_DelegatesToExecutor()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        var systemUnderTests = new RobotController(executor);

        // Act
        systemUnderTests.FillCell();

        // Assert
        A.CallTo(() => executor.FillCell()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IsCellFilled_ReturnsExecutorValue()
    {
        // Arrange
        var executor = A.Fake<IRobotExecutor>();
        A.CallTo(() => executor.IsCellFilled()).Returns(true);
        var systemUnderTests = new RobotController(executor);

        // Act
        var isFilled = systemUnderTests.IsCellFilled();

        // Assert
        isFilled.Should().BeTrue();
        A.CallTo(() => executor.IsCellFilled()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Executor_WhenReplaced_UsesNewExecutor()
    {
        // Arrange
        var firstExecutor = A.Fake<IRobotExecutor>();
        var secondExecutor = A.Fake<IRobotExecutor>();
        var systemUnderTests = new RobotController(firstExecutor);
        systemUnderTests.Executor = secondExecutor;

        // Act
        systemUnderTests.MoveLeft();

        // Assert
        A.CallTo(() => firstExecutor.Move(A<RobotDirection>._)).MustNotHaveHappened();
        A.CallTo(() => secondExecutor.Move(RobotDirection.Left)).MustHaveHappenedOnceExactly();
    }
}
