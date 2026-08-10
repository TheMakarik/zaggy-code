import sys
import imp

module_name = 'robot'
module_path = robot_path

module = imp.new_module(module_name)

with open(module_path, 'r', encoding='utf-8') as f:
    code = f.read()

exec(code, module.__dict__)

for name in list(globals().keys()):
    if name.startswith('clr_'):
        setattr(module, name, globals()[name])

sys.modules[module_name] = module