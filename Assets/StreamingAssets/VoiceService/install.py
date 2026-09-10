"""Install/update the local service in the standard macOS application support folder.
Run with the project's existing Python environment after downloading the models.
"""
from pathlib import Path
import shutil
source=Path(__file__).resolve().parent
repo=source.parents[2]
support=Path.home()/'Library/Application Support/BB8Voice'
support.mkdir(parents=True,exist_ok=True)
local=repo/'.local-voice'
if not (support/'.local-voice').exists():
 if not local.exists():raise SystemExit('Primero crea .local-voice/venv y descarga los modelos con setup.py.')
 shutil.move(str(local),str(support/'.local-voice'))
 local.symlink_to(support/'.local-voice',target_is_directory=True)
(support/'VoiceService').mkdir(exist_ok=True)
shutil.copy2(source/'service.py',support/'VoiceService/service.py')
shutil.copy2(source/'audio_input.py',support/'VoiceService/audio_input.py')
print('Servicio instalado en',support)
