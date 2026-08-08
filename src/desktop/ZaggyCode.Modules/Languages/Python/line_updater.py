from sys import settrace
from time import sleep

def on_new_line(line_number):
    clr_raise_debug_line_updated(line_number)  # type: ignore
   

def trace_calls(frame, event, arg):
    if event == 'line':
        on_new_line(frame.f_lineno)
    return trace_calls

settrace(trace_calls)