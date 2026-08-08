"""
Zaggy's Code - Python robot модуль
Вызывает методы из RobotExecutor, которые получает из IronPython
"""

def move_up():
    clr_RobotExecutor_MoveUp()  # type: ignore

def move_down():
    clr_RobotExecutor_MoveDown()  # type: ignore

def move_right():
    clr_RobotExecutor_MoveRight()  # type: ignore

def move_left():
    clr_RobotExecutor_MoveLeft()  # type: ignore

def is_wall_from_up():
    return clr_RobotExecutor_IsWallFromUp()  # type: ignore

def is_wall_from_down():
    return clr_RobotExecutor_IsWallFromDown()  # type: ignore

def is_wall_from_right():
    return clr_RobotExecutor_IsWallFromRight()  # type: ignore

def is_wall_from_left():
    return clr_RobotExecutor_IsWallFromLeft()  # type: ignore

def fill_cell():
    clr_RobotExecutor_FillCell()  # type: ignore

def is_cell_filled():
    return clr_RobotExecutor_IsCellFilled()  # type: ignore