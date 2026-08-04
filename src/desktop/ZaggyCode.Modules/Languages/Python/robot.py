import typing

def move_up() -> None:
    __clr_RobotExecutor_MoveUp() # type: ignore

def move_down() -> None:
    __clr_RobotExecutor_MoveDown() # type: ignore

def move_right() -> None:
    __clr_RobotExecutor_MoveRight() # type: ignore

def move_left() -> None:
    __clr_RobotExecutor_MoveLeft() # type: ignore

def is_wall_from_up() -> bool:
    return __clr_RobotExecutor_IsWallFromUp() # type: ignore

def is_wall_from_down() -> bool:
    return __clr_RobotExecutor_IsWallFromDown() # type: ignore

def is_wall_from_right() -> bool:
    return __clr_RobotExecutor_IsWallFromRight() # type: ignore

def is_wall_from_left() -> bool:
    return __clr_RobotExecutor_IsWallFromLeft() # type: ignore

def fill_cell() -> None:
    __clr_RobotExecutor_FillCell() #type: ignore

def is_cell_filled() -> bool:
    return  __clr_RobotExecutor_IsCellFilled()  #type: ignore
