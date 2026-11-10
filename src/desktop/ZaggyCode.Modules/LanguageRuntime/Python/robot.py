"""
Zaggy's Code - Python robot модуль
Вызывает методы из RobotController, которые получает из IronPython
"""

def move_up():
    clr_RobotController_MoveUp()  # type: ignore

def move_down():
    clr_RobotController_MoveDown()  # type: ignore

def move_right():
    clr_RobotController_MoveRight()  # type: ignore

def move_left():
    clr_RobotController_MoveLeft()  # type: ignore

def is_wall_from_up():
    return clr_RobotController_IsWallFromUp()  # type: ignore

def is_wall_from_down():
    return clr_RobotController_IsWallFromDown()  # type: ignore

def is_wall_from_right():
    return clr_RobotController_IsWallFromRight()  # type: ignore

def is_wall_from_left():
    return clr_RobotController_IsWallFromLeft()  # type: ignore

def fill_cell():
    clr_RobotController_FillCell()  # type: ignore

def is_cell_filled():
    return clr_RobotController_IsCellFilled()  # type: ignore
