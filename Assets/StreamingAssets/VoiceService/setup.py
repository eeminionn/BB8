"""Run with the local venv to download the two free models once."""
from pathlib import Path
from huggingface_hub import snapshot_download
root=Path(__file__).resolve().parents[3]/'.local-voice/models'
for repo,name in [('mlx-community/Qwen3-4B-Instruct-2507-4bit','llm'),('mlx-community/whisper-small-mlx','stt')]:
 snapshot_download(repo,local_dir=root/name)
