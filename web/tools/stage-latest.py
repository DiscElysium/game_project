"""Stage the actual WebGL output with content-hashed names; retain the old build outside publication."""
from pathlib import Path
import hashlib,json,shutil
root=Path(__file__).resolve().parents[1]; site=root/'game-web-host'; public=site/'public'
source=root/'webgl-output/latest/Build'; target=public/'Build'
files=list(source.iterdir()); assert files,'WebGL build missing'
if target.exists():
    backup=site/'.sites-runtime/build-before-r3'
    assert target.resolve().is_relative_to(site.resolve()) and backup.resolve().is_relative_to(site.resolve())
    if not backup.exists(): shutil.move(str(target),str(backup))
target.mkdir(exist_ok=True)
manifest={'version':'2026.10.10-r3-web'}
for file in files:
    kind='loader' if file.name.endswith('.loader.js') else 'framework' if '.framework.js' in file.name else 'data' if '.data.' in file.name else 'wasm' if '.wasm.' in file.name else None
    if not kind: continue
    sha=hashlib.sha256(file.read_bytes()).hexdigest(); name=sha[:12]+'-'+file.name
    shutil.copy2(file,target/name); manifest[kind]='Build/'+name
    print(kind,file.stat().st_size,sha[:12])
assert all(key in manifest for key in ['loader','framework','data','wasm'])
(public/'release.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
shutil.copy2(root/'website/dist/assets/pixel.woff2',public/'pixel.woff2')
shutil.copy2(root/'gamejam1bitDeepSeek/Validation/WorldExpansion/Runtime/masks-front.png',public/'masks-r3.png')
html='''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="theme-color" content="#2c1b17"><title>Masked Dice — Play</title><link rel="stylesheet" href="player-v3.css"></head><body><canvas id="unity-canvas" width="1280" height="720" tabindex="0" aria-label="Masked Dice: physical table controls. TAB observes, right mouse looks, F2 picture, F3 AI lab."></canvas><div id="loading"><div class="loading-card"><h1>MASKED DICE</h1><img src="masks-r3.png" alt="Watcher, Weaver and Risker masks"><p id="loading-message">Preparing the tribunal…</p><progress id="progress" max="1" value="0"></progress><p>Keyboard + mouse recommended.<br>TAB observe / table · RMB look · F2 picture · F3 AI lab</p><button id="retry" hidden>RETRY</button></div></div><button id="fullscreen" hidden>FULLSCREEN</button><script src="player-v3.js"></script></body></html>'''
(public/'play.html').write_text(html,encoding='utf-8')
print('LATEST_WEBGL_STAGED',manifest['version'])
