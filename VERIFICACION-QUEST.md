# Verificación Quest 2 — 10 de septiembre de 2026

- Unity Hub terminó de instalar Android Build Support, OpenJDK, NDK, CMake y herramientas SDK para Unity 6000.5.8f1.
- Se compiló el APK ARM64/IL2CPP y se inspeccionaron identificador, actividad de inicio, SDK mínimo/objetivo y permiso de micrófono. El paquete es `com.eeminionn.bb8.quest`.
- ADB instaló el APK en el Quest 2 conectado y el sistema abrió `UnityPlayerActivity` después de activar los mandos.
- OpenXR creó la sesión estéreo y llegó al estado `FOCUSED`. El registro propio confirmó el seguimiento de cabeza, A, ambos gatillos, X y B.
- El servicio del Mac respondió `ready`, se estableció la redirección USB del puerto 8768 y el cliente dentro del visor confirmó `BB8_QUEST_VOICE_READY`.
- El permiso de micrófono fue concedido por el usuario dentro del visor.
- En la muestra de ejecución observada, el compositor del Quest informó `FPS=72/72`, sin fotogramas obsoletos (`Stale=0`) y aproximadamente 2,7–2,9 ms de GPU de la aplicación. Esto describe la muestra observada, no una garantía de rendimiento en todos los recorridos.

Las comprobaciones automatizadas cubren compilación, instalación, carga de OpenXR, conexión de voz y recepción de botones. La legibilidad y comodidad dentro del visor, el desplazamiento, el salto y la reacción a frases del usuario requieren la prueba interactiva solicitada al usuario; no se sustituyen por los registros.

El registro de arranque incluye una consulta opcional de Unity a `AssetPackManager` que informa que la clase no está disponible. La aplicación sigue arrancando y renderizando; los recursos están incluidos en el APK, sin entrega de recursos de Google Play. No se observó un cierre por esa consulta.

El sonido de la aplicación inicia desactivado y el Mac solo ejecuta el servicio de interpretación. Las pruebas no activaron sonidos de los personajes en el computador.
