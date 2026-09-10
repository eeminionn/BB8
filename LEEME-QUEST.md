# BB8 en Meta Quest 2

El entorno 3D se ejecuta directamente en el Quest 2. El Mac procesa la transcripción y la emoción con los modelos locales existentes. La conexión de voz usa USB; no necesita una API de pago ni envía audio a Internet. Meta Link para PC requiere Windows, por lo que esta versión usa Android/OpenXR nativo.

## Iniciar

1. Conecta el Quest al Mac por USB, enciende el visor y despierta ambos mandos Touch.
2. Acepta la depuración USB en el visor. «Permitir siempre desde este computador» evita repetir el permiso cuando Unity reinicia ADB.
3. Abre **Iniciar BB8 VR.command**, junto a **BB8-Quest.apk** y **BB8.app**. El iniciador conecta la voz, abre BB8 en el visor y abre la vista duplicada en tu navegador. También puedes ejecutar `python3 Tools/quest_bridge.py` desde este repositorio.
4. Dentro del menú, elige Khepra o Tierra con el joystick izquierdo y pulsa A. Si el sistema del visor muestra un aviso sobre mandos, complétalo allí antes de entrar.

La primera vez que hables, permite el micrófono del visor si lo solicita. Si desconectas y reconectas el cable, vuelve a abrir el iniciador para restaurar el enlace de voz. La exploración continúa disponible sin el servicio del Mac; la conversación requiere esa conexión. El servicio local puede tardar en cargar los modelos.

## Controles

| Mando | Acción |
| --- | --- |
| Joystick izquierdo, arriba/abajo | Avanzar/retroceder relativo a dónde miras |
| Joystick izquierdo, izquierda/derecha | Girar; se puede avanzar y girar a la vez |
| A | Saltar; en el menú, entrar o continuar |
| Gatillo derecho, mantener y soltar | Grabar voz y enviar la frase a BB8 |
| Gatillo izquierdo + movimiento | Turbo al controlar BB8; consume su batería |
| X | Cambiar entre Minion y BB8 |
| Joystick derecho, izquierda/derecha | Girar sin desplazarse; el eje vertical no tiene acción |
| B | Abrir/cerrar el menú |
| Y, dentro del menú | Activar/desactivar el sonido del visor |
| Y, durante la exploración | Cambiar primera/tercera persona |

La cámara comienza en primera persona. Y alterna una vista de tercera persona con detección de obstáculos detrás del personaje. Ambas mantienen el seguimiento natural del visor y el horizonte nivelado. Los dos joysticks permiten giro continuo a un máximo conjunto de 60°/s, con zona muerta para evitar deriva. No se duplica la velocidad al girar con ambos a la vez.

El menú conserva los tonos dorados y azules, con texto independiente de la iluminación del escenario y panel opaco para no tapar las letras con transparencias. Los planetas son esferas 3D con relieve de color e iluminación estilizada, visibles en estéreo y girando incluso en pausa. Los mandos se muestran a escala real, siguen las manos y resaltan los botones activos; al mirarlos aparecen sus funciones.

## Micrófono

La aplicación Android usa el micrófono seleccionado por el sistema del Quest, normalmente el integrado del visor, no el del Mac. El Mac solo transcribe y clasifica. La captura solicita 48 kHz cuando el dispositivo lo permite; espera a recibir muestras antes de indicar «Te escucho» y muestra un medidor de nivel. Al soltar el gatillo conserva 120 ms finales para evitar cortar la última sílaba.

Antes de Whisper, el servicio elimina componente continua y ruido grave bajo 70 Hz, ajusta el volumen con ganancia máxima ×6 y remuestrea a 16 kHz. Se mantiene el filtro de segmentos de Whisper para evitar transcribir silencios. Un medidor bajo ayuda a detectar una señal débil, pero un error de transcripción no diagnostica un fallo del micrófono. No se graba continuamente ni se guardan los audios.

El sonido comienza apagado. Activarlo en el visor no reproduce sonidos en el Mac: la vista duplicada no recibe audio.

## Vista del Quest en el Mac

El iniciador prepara [la ventana local](http://127.0.0.1:8769/). Duplica la vista del ojo izquierdo del juego, incluidos su menú, mandos y textos. No captura los menús del sistema de Meta ni el entorno real. Es una vista de acompañamiento de hasta 10 imágenes/s a 640 × 704; la imagen del visor mantiene su frecuencia independiente.

Se reutilizan la posición y proyección del ojo izquierdo. Solo se dibuja la cámara adicional si hay un navegador solicitando imágenes. Al cerrar la página o dejar su pestaña oculta, se suspende en un máximo de tres segundos. Solo el último JPEG permanece en memoria, sin archivos de vídeo ni audio. El servidor escucha en `127.0.0.1:8769`, recibe por `adb reverse` y exige la misma clave local del dispositivo para recibir imágenes. No necesita Internet ni suscripción.

La captura externa con scrcpy no resultó compatible con los permisos de captura de este firmware. Por eso la versión final emite únicamente su propio contenido desde Unity. No se alteraron permisos del sistema del visor. `--no-mirror` omite el visor del Mac y `--no-open` prepara su servidor sin abrir el navegador.

## Desarrollo

Se usan Unity 6000.5.8f1, Android Build Support (SDK/NDK/OpenJDK) instalado desde Unity Hub, OpenXR 1.16.1, Oculus Touch Controller Profile y Meta Quest Support. APK ARM64/IL2CPP, Vulkan, renderizado estéreo Single Pass Instanced, Android mínimo 29 y SDK objetivo 36. Se usa una compilación de desarrollo para inspeccionar errores y los controles en el visor; no es una publicación en la tienda.

`Tools/build_quest.sh` configura Android, compila `../BB8-Quest.apk` y restaura la configuración de escritorio. `python3 Tools/quest_bridge.py --install` instala esa compilación en el único dispositivo Android autorizado conectado. El editor debe estar cerrado para usar la compilación por consola.

La clave de voz se copia al directorio privado de la aplicación en el visor, sin incluirla en el APK ni en Git. `adb reverse tcp:8768 tcp:8768` transporta las solicitudes hacia el servicio del Mac, que sigue escuchando exclusivamente en `127.0.0.1`. La configuración Android permite HTTP únicamente para localhost. No se guardan audio ni transcripciones.

Referencias: [configuración de hardware de Meta](https://developers.meta.com/horizon/design/prototype-setup-hardware/) y [OpenXR de Unity](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.16/manual/index.html).
