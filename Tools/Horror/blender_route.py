"""Fail-closed Safe Mode client for the Unity project's existing blender_unity MCP socket."""
import importlib.util
import json
from pathlib import Path
import socket
import sys

ROOT = Path(__file__).resolve().parents[2]
MASTER = str(ROOT / 'UnityMinecraft.blend').replace('\\', '/').lower()
VALIDATORS = list((Path.home() / 'AppData/Local/uv/cache/archive-v0').glob('*/blender_mcp/safe_mode.py'))
if not VALIDATORS:
    raise RuntimeError('Installed Blender MCP Safe Mode validator not found')
spec = importlib.util.spec_from_file_location('horror_installed_safe_mode', VALIDATORS[0])
validator = importlib.util.module_from_spec(spec)
spec.loader.exec_module(validator)

def send(kind, params):
    with socket.create_connection(('127.0.0.1', 9876), timeout=30) as sock:
        sock.settimeout(300)
        sock.sendall(json.dumps({'type': kind, 'params': params}).encode())
        data = b''
        while True:
            part = sock.recv(65536)
            if not part:
                raise RuntimeError('Blender disconnected before result')
            data += part
            try:
                response = json.loads(data)
                if response.get('status') == 'error':
                    raise RuntimeError(response.get('message'))
                return response
            except json.JSONDecodeError:
                pass

def execute(code):
    validator.validate_code(code)
    return send('execute_code', {'code': code})

def verify():
    response = execute('import bpy\nprint("HORROR_MASTER=" + bpy.data.filepath)')
    raw = json.dumps(response).replace('\\\\', '/').lower()
    if MASTER not in raw:
        raise RuntimeError('Wrong Blender master: ' + json.dumps(response))
    return response

if __name__ == '__main__':
    print(json.dumps(verify(), ensure_ascii=False))
    if len(sys.argv) > 1:
        code = Path(sys.argv[1]).read_text(encoding='utf-8')
        print(json.dumps(execute(code), ensure_ascii=False))
