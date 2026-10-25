"""Create a browser project from the verified desktop release, without desktop AI binaries or credentials."""
from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[1]
source=root/'gamejam1bitDeepSeek'; target=root/'gamejam1bitWeb'
assert not target.exists(),'Browser project already exists; do not overwrite it blindly'
target.mkdir()
shutil.copytree(source/'Assets',target/'Assets',ignore=shutil.ignore_patterns('LLMUnity','LLMUnity.meta','StreamingAssets','StreamingAssets.meta','*.dpapi','*.gguf'))
for folder in ['Packages','ProjectSettings']:
    shutil.copytree(source/folder,target/folder)
(target/'Validation').mkdir()
print('LATEST_BROWSER_PROJECT_READY',target)
