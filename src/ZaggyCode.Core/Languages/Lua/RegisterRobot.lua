
local robot = {};

function robot.move_up()
    __clr_RobotExecutor:MoveUp()
end

function robot.move_right()
    __clr_RobotExecutor:MoveRight()
end

function robot.move_down()
    __clr_RobotExecutor:MoveDown()
end

function robot.move_left()
    __clr_RobotExecutor:MoveLeft()
end

function robot.can_move_up()
    __clr_RobotExecutor:CanMoveUp()
end

function robot.can_move_right()
    __clr_RobotExecutor:CanMoveRight()
end

function robot.can_move_down()
    __clr_RobotExecutor:CanMoveDown()
end

function robot.can_move_left()
    __clr_RobotExecutor:CanMoveLeft()
end

function robot.draw()
    __clr_RobotExecutor:Draw()
end

function robot.is_wall_from_up()
    __clr_RobotExecutor:IsWallFromUp()
end

function robot.is_wall_from_down()
    __clr_RobotExecutor:IsWallFromDown()
end

function robot.is_wall_from_left()
    __clr_RobotExecutor:IsWallFromLeft()
end

function robot.is_wall_from_right()
    __clr_RobotExecutor:IsWallFromRight()
end

package.preload["robot"] = robot;