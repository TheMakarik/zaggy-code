import sys

module_name = 'robot'
module = type(sys)('robot') 

with open(robot_path, 'r', encoding='utf-8') as f:
    code = f.read()

exec(code, module.__dict__)
sys.modules[module_name] = module
