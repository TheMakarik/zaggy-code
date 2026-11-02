import sys
import builtins

class R:
    def __init__(self):
        builtins.print = self._print
        builtins.input = self._input

    def _print(self, *args, sep=' ', end='\n', file=None, flush=False):
        text = sep.join(str(a) for a in args) + end
        clr_output(text)  # type: ignore

    def _input(self, prompt=''):
        if prompt:
            clr_output(prompt)  # type: ignore
        return clr_input()  # type: ignore

r = R()