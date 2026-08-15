import sys
sys.settrace(None)

frame = sys._getframe()
while frame is not None:
    frame.f_trace = None
    frame = frame.f_back