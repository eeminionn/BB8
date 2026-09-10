#!/usr/bin/env python3
"""Connect Quest to the existing local voice service over authorized USB ADB only."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
VOICE = Path.home() / 'Library/Application Support/BB8Voice'
PACKAGE = 'com.eeminionn.bb8.quest'


def health():
    try:
        with urllib.request.urlopen('http://127.0.0.1:8768/health', timeout=2) as response:
            return json.load(response)
    except (OSError, ValueError):
        return None


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--install', action='store_true', help='Install/update the built APK first')
    args = parser.parse_args()
    bundled = Path('/Applications/Unity/Hub/Editor/6000.5.8f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb')
    adb = str(bundled) if bundled.is_file() else shutil.which('adb') or str(Path.home() / 'Library/Android/sdk/platform-tools/adb')
    # Match Unity's ADB version and allow the USB authorization handshake to finish.
    ready = []
    for attempt in range(10):
        devices = subprocess.check_output([adb, 'devices'], text=True).splitlines()[1:]
        ready = [line.split()[0] for line in devices if len(line.split()) > 1 and line.split()[1] == 'device']
        if ready:
            break
        time.sleep(1)
    if len(ready) != 1:
        raise RuntimeError('Conecta solo el Quest por USB y acepta «Permitir depuración USB» dentro del visor.')
    command = [adb, '-s', ready[0]]

    def run(*arguments):
        return subprocess.run(command + list(arguments), check=True, capture_output=True, text=True)

    if args.install:
        print('Instalando BB8 en el Quest…', flush=True)
        run('install', '-r', str(ROOT.parent / 'BB8-Quest.apk'))
    result = health()
    if result and result.get('service') != 'bb8-local-voice':
        raise RuntimeError('El puerto 8768 está ocupado por otro servicio.')
    if result is None:
        python = VOICE / '.local-voice/venv/bin/python'
        script = VOICE / 'VoiceService/service.py'
        if not python.is_file() or not script.is_file():
            raise RuntimeError('No se encontró la instalación de voz local de BB8 en este Mac.')
        environment = os.environ.copy()
        environment['BB8_PROJECT'] = str(VOICE)
        with (VOICE / '.local-voice/quest-service.log').open('ab') as log:
            subprocess.Popen([str(python), str(script)], cwd=VOICE, env=environment,
                             stdout=log, stderr=log, stdin=subprocess.DEVNULL, start_new_session=True)
    print('Preparando la voz local en el Mac…', flush=True)
    for _ in range(180):
        result = health()
        if result and result.get('error'):
            raise RuntimeError('No se pudieron cargar los modelos locales. Revisa el registro de voz.')
        if result and result.get('service') == 'bb8-local-voice' and result.get('ready'):
            break
        time.sleep(1)
    else:
        raise RuntimeError('La voz aún no termina de cargar. Vuelve a abrir este iniciador en un minuto.')
    run('reverse', 'tcp:8768', 'tcp:8768')
    remote = f'/sdcard/Android/data/{PACKAGE}/files'
    run('shell', 'mkdir', '-p', remote)
    # The token is provisioned to this authorized device, never included in source or APK.
    run('push', str(VOICE / '.local-voice/token'), remote + '/quest-voice-token')
    run('shell', 'am', 'start', '-n', PACKAGE + '/com.unity3d.player.UnityPlayerActivity')
    print('Conexión preparada. Continúa dentro del visor; acepta allí cualquier aviso sobre los mandos.\n'
          'Mantén conectado el cable USB para conversar.\n'
          'Izquierdo: mover · A: saltar · Gatillo derecho: hablar · Izquierdo: turbo BB8\n'
          'X: cambiar personaje · B: menú · Joystick derecho: giro de 30°.\n'
          'El sonido empieza apagado. Puedes activarlo con Y dentro del menú del visor.\n'
          'Si desconectas el cable, vuelve a abrir este iniciador al conectarlo.', flush=True)


if __name__ == '__main__':
    try:
        main()
    except (RuntimeError, subprocess.CalledProcessError, OSError) as error:
        print('No se pudo conectar BB8 VR: ' + str(error), file=sys.stderr)
        sys.exit(1)
