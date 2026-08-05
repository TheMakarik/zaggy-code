from sys import settrace
from os import sleep

def on_new_line(line_number: int):
    __clr_raise_debug_line_updated(line_number)  #type: ignore
    sleep(__clr_wait_to_new_line_ms) #type: ignore

def trace_calls(frame, event, arg):
    if event == 'line':
        on_new_line(frame.f_lineno)
    return trace_calls

settrace(trace_calls);
print("Successfully loaded new line updater")