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

## Interfaz, mandos, voz y vista del Mac

- Se recompiló e instaló el APK con planetas geométricos, consola opaca y texto dorado. La comprobación visual fuera de pantalla detectó y permitió corregir el panel transparente que oscurecía las letras. `QuestPresentationChecks.Run` genera vistas de ambos destinos y comprueba la escala física de ambos modelos.
- El usuario confirmó que el menú se ve mucho mejor, que su encuadre más alejado y desplazado hacia los textos es cómodo, y que Y alterna correctamente primera/tercera persona.
- La corrección final de mandos conserva la rotación y escala importadas del FBX y aplica por encima la conversión de dirección. Se comprobaron los centros geométricos frente al GLB: botones sobre el gatillo, gatillo hacia delante y mandos de aproximadamente 10 × 17 × 18 cm. La lectura OpenXR confirmó cabeza y ambos mandos con pose válida. Después de instalar la corrección final, el usuario confirmó que los botones quedan hacia arriba y los mandos están bien orientados.
- La versión final arrancó sin excepciones de los componentes del proyecto. Continúa la consulta opcional de Unity a `AssetPackManager` descrita arriba.
- `Tools/check_audio_input.py`: 4 pruebas aprobadas (voz baja estéreo, silencio/componente continua/datos inválidos, límite de amplitud y duración). Una frase sintética española a RMS 0,002 y 48 kHz se transcribió completa y fue clasificada como alegría amistosa, intensidad 3, en 1,32 s en esta prueba. Es una prueba de la cadena de señal, no una medición del hardware del micrófono del usuario.
- `Tools/check_quest_viewer.py`: 4 pruebas aprobadas (presencia/expiración del navegador, token obligatorio, rechazo de datos inválidos y conservación solo del último fotograma sin caché). La carga sin token también fue rechazada con HTTP 403 en el servicio real.
- Se verificó visualmente en el navegador del Mac la ciudad y Khepra emitidos por la cámara del juego, con indicador **EN DIRECTO**, sin audio. No se capturan pantallas del sistema de Meta ni se cambiaron sus permisos de captura.
- La compresión JPEG se trasladó a un trabajo de CPU secundario, con un único envío pendiente. Con la duplicación activa, los últimos 20 registros de una muestra estable informaron 71–73 FPS, con mediana 72. La cifra varía con el escenario y las transiciones; no garantiza esa frecuencia en todos los recorridos.
- La configuración de escritorio se restauró a sus valores anteriores después de compilar Android. `git diff --check` pasó.
