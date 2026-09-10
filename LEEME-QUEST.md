# BB8 en Meta Quest 2

El entorno 3D se ejecuta directamente en el Quest 2. El Mac procesa la transcripción y la emoción con los modelos locales existentes. La conexión de voz usa USB; no necesita una API de pago ni envía audio a Internet. Meta Link para PC requiere Windows, por lo que esta versión usa Android/OpenXR nativo.

## Iniciar

1. Conecta el Quest al Mac por USB, enciende el visor y despierta ambos mandos Touch.
2. Acepta la depuración USB en el visor. «Permitir siempre desde este computador» evita repetir el permiso cuando Unity reinicia ADB.
3. Abre **Iniciar BB8 VR.command**, junto a **BB8-Quest.apk** y **BB8.app**. El iniciador conecta la voz y solicita abrir BB8 en el visor. También puedes ejecutar `python3 Tools/quest_bridge.py` desde este repositorio.
4. Dentro del menú, elige Khepra o Tierra con el joystick izquierdo y pulsa A. Si el sistema del visor muestra un aviso sobre mandos, complétalo allí antes de entrar.

La primera vez que hables, permite el micrófono del visor si lo solicita. Si desconectas y reconectas el cable, vuelve a abrir el iniciador para restaurar el enlace de voz. La exploración continúa disponible sin el servicio del Mac; la conversación requiere esa conexión. El servicio local puede tardar en cargar los modelos.

## Controles

| Mando | Acción |
| --- | --- |
| Joystick izquierdo | Desplazamiento relativo a dónde miras |
| A | Saltar; en el menú, entrar o continuar |
| Gatillo derecho, mantener y soltar | Grabar voz y enviar la frase a BB8 |
| Gatillo izquierdo + movimiento | Turbo al controlar BB8; consume su batería |
| X | Cambiar entre Minion y BB8 |
| Joystick derecho | Giro de 30° por paso; vuelve al centro para repetir |
| B | Abrir/cerrar el menú |
| Y, dentro del menú | Activar/desactivar el sonido del visor |

La cámara VR es en primera persona, con seguimiento de cabeza y sin el giro automático de la cámara de escritorio. El movimiento del cuerpo y los saltos desplazan al observador, pero no inclinan ni hacen rodar el horizonte. El menú y la emoción de BB8 son elementos del mundo 3D visibles para ambos ojos. Los planetas giran también con la simulación pausada.

El sonido comienza apagado. Activarlo en el visor no reproduce sonidos en el Mac: allí solo se ejecuta el procesamiento de voz.

## Desarrollo

Se usan Unity 6000.5.8f1, Android Build Support (SDK/NDK/OpenJDK) instalado desde Unity Hub, OpenXR 1.16.1, Oculus Touch Controller Profile y Meta Quest Support. APK ARM64/IL2CPP, Vulkan, renderizado estéreo Single Pass Instanced, Android mínimo 29 y SDK objetivo 36. Se usa una compilación de desarrollo para inspeccionar errores y los controles en el visor; no es una publicación en la tienda.

`Tools/build_quest.sh` configura Android, compila `../BB8-Quest.apk` y restaura la configuración de escritorio. `python3 Tools/quest_bridge.py --install` instala esa compilación en el único dispositivo Android autorizado conectado. El editor debe estar cerrado para usar la compilación por consola.

La clave de voz se copia al directorio privado de la aplicación en el visor, sin incluirla en el APK ni en Git. `adb reverse tcp:8768 tcp:8768` transporta las solicitudes hacia el servicio del Mac, que sigue escuchando exclusivamente en `127.0.0.1`. La configuración Android permite HTTP únicamente para localhost. No se guardan audio ni transcripciones.

Referencias: [configuración de hardware de Meta](https://developers.meta.com/horizon/design/prototype-setup-hardware/) y [OpenXR de Unity](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.16/manual/index.html).
