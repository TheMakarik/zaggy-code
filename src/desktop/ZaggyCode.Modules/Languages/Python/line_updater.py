import sys

def on_new_line(line_number: int):
    """Пустой метод, вызываемый при переходе на новую строку"""
    pass

def trace_calls(frame, event, arg):
    if event == 'line':
        on_new_line(frame.f_lineno)
    return trace_calls

sys.settrace(trace_calls)
