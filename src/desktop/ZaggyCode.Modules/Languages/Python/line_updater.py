from sys import settrace
import sys

def on_new_line(line_number):
    clr_raise_debug_line_updated(line_number)  # type: ignore

def trace_calls(frame, event, arg):
    clr_try_cancel_execution() # type: ignore
    if event == 'line':
        filename = frame.f_code.co_filename
        if filename != '<string>' and not filename.startswith('<'):
            return trace_calls
        if frame.f_code.co_name in ('trace_calls', 'on_new_line', '<module>'):
            if frame.f_code.co_filename == '<string>':
                on_new_line(frame.f_lineno)
    return trace_calls

settrace(trace_calls)