"""Install only the expansion player beside the preserved base player, with matching shared runtime."""
from pathlib import Path
import hashlib
import shutil

root=Path(__file__).resolve().parents[2]
source=root/'Builds/HorrorRelease'
if not (source/'MinecraftHorror.exe').is_file():raise RuntimeError('Release player missing')
for path in source.iterdir():
    if path.name.startswith('MinecraftHorror') or path.name.endswith('BackUpThisFolder_ButDontShipItWithYourGame'):continue
    target=root/path.name
    if path.is_file():
        if target.exists() and hashlib.sha256(path.read_bytes()).digest()!=hashlib.sha256(target.read_bytes()).digest():raise RuntimeError('Different preserved shared runtime: '+path.name)
    else:
        for file in path.rglob('*'):
            if file.is_file():
                counterpart=target/file.relative_to(path)
                if counterpart.exists() and hashlib.sha256(file.read_bytes()).digest()!=hashlib.sha256(counterpart.read_bytes()).digest():raise RuntimeError('Different preserved runtime component: '+str(file.relative_to(source)))
for path in source.iterdir():
    if path.name.endswith('BackUpThisFolder_ButDontShipItWithYourGame'):continue
    target=root/path.name
    if path.is_dir():shutil.copytree(path,target,dirs_exist_ok=True)
    elif not target.exists() or path.name=='MinecraftHorror.exe':shutil.copy2(path,target)
print('Root player installed; preserved base executable/shared runtime not changed')
