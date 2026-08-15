import sys
import imp

module_name = 'robot'
module_path = robot_path

class FakeAsyncio:
    def __getattr__(self, name):
        raise ImportError(
            f"Module 'asyncio' is not supported in this environment "
            f"Attempted to access: {name}"
        )

    def __call__(self, *args, **kwargs):
        raise ImportError("Module 'asyncio' is not supported in this environment")

fake_asyncio = FakeAsyncio()
sys.modules['asyncio'] = fake_asyncio

if module_name in sys.modules:
    module = sys.modules[module_name]
else:
    module = imp.new_module(module_name)

    with open(module_path, 'r', encoding='utf-8') as f:
        code = f.read()
    exec(code, module.__dict__)
    sys.modules[module_name] = module

for name in list(globals().keys()):
    if name.startswith('clr_'):
        setattr(module, name, globals()[name])