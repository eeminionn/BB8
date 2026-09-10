#!/bin/zsh
set -eu
BB8_PROJECT_DIR="${0:A:h:h}"
BB8_UNITY_EDITOR="${BB8_UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.5.8f1/Unity.app/Contents/MacOS/Unity}"
cd -- "$BB8_PROJECT_DIR"
mkdir -p Logs
restore_desktop() {
  "$BB8_UNITY_EDITOR" -batchmode -noaudio -projectPath "$BB8_PROJECT_DIR" -buildTarget OSXUniversal -executeMethod QuestBuild.RestoreDesktopInput -quit -logFile Logs/quest-restore-desktop.log
}
trap restore_desktop EXIT
"$BB8_UNITY_EDITOR" -batchmode -noaudio -projectPath "$BB8_PROJECT_DIR" -buildTarget Android -executeMethod QuestBuild.Configure -quit -logFile Logs/quest-configure.log
"$BB8_UNITY_EDITOR" -batchmode -noaudio -projectPath "$BB8_PROJECT_DIR" -buildTarget Android -executeMethod QuestBuild.Build -quit -logFile Logs/quest-build.log
print 'APK listo en ../BB8-Quest.apk'
