# BB8 · Desguace 08

Escena de exploración libre para Unity 6000.5.8f1, probada en Apple M5. Un pequeño desguace espacial para recorrer con BB8, sin objetivos, puntuación ni sistema de progresión.

## Abrir

- En Unity: menú **BB8 > Open Demo**, luego **Play** y clic dentro de Game.
- Sin editor: abre **BB8.app**, situada junto a esta carpeta. Command-Q cierra la aplicación.
- Desde Unity Hub: **Add project from disk**, selecciona esta carpeta y usa Unity 6000.5.8f1.

La escena principal es `Assets/scrapyard.unity`. La antigua pista de prueba se conserva en `Assets/demo.unity`.

## Controles

| Entrada | Acción |
| --- | --- |
| WASD | Rodar |
| Mouse | Orientar la cámara |
| Rueda | Acercar o alejar |
| Espacio | Saltar; pausa de 0,65 segundos entre saltos |
| Shift + dirección | Turbo, hasta 1,4 segundos por ráfaga |
| R | Volver al punto inicial y reiniciar la escena |

El turbo consume aproximadamente un 70 % de batería por ráfaga completa. Suelta Shift antes de iniciar otra; hay una pausa de 0,35 segundos. Hace falta al menos un 12 % de carga para arrancar. Las seis celdas azules recargan un 55 %, sin superar el 100 %, y reaparecen tras 16 segundos. Con batería llena no se consumen.

## Detalles

- Voces sintéticas originales con variantes de salto y choque; sonidos discretos de turbo y recarga. Los golpes leves y los contactos continuos no repiten la voz.
- Pequeños gestos de cabeza al detenerse y vibración de antenas desde su base. La animación visual es independiente del estabilizador físico de la cabeza.
- Casco abierto de carguero, motores, alas caídas, contenedores, conductos transitables y pasarela con una pequeña separación para cruzar con impulso y salto.
- Ambiente de arena y metal, luz cálida, reflejo ambiental explícito y viento suave. La geometría y los materiales están guardados como recursos de Unity; no se construye el escenario en cada arranque.
- Cámara que evita atravesar paredes y un cambio moderado de campo de visión durante el turbo.
- Si BB8 cae fuera del suelo, vuelve al inicio.

## Ajustes y mantenimiento

Selecciona **BB8/body** para ajustar velocidad, duración/consumo del turbo, sonidos y gestos. Las celdas exponen carga y tiempo de reaparición.

**BB8 > Build Mac App** reconstruye `../BB8.app`. **BB8 > Verify Scrapyard** ejecuta pruebas de físicas e interacción y escribe el resultado en `Logs/scrapyard-verification.txt`; los objetos temporales de prueba no se guardan ni se incluyen en la aplicación.

**BB8 > Create Scrapyard Scene** reconstruye la escena de exploración y sobrescribe su distribución. Para editarla manualmente, basta con guardar los cambios habituales; no es necesario ejecutar ese comando.

Blender 5.2.1 está instalado en `/Applications/Blender.app` para importar los modelos originales `.blend`. No se añadieron paquetes externos, servicios online ni dependencias de pago.

Proyecto original: https://github.com/mattolenik/BB8
Fork: https://github.com/eeminionn/BB8
Rama de esta versión: `scrapyard-experience`.

## Conversación local y Minion

La escena comienza controlando al Minion. **Tab** alterna el control y la cámara entre Minion y BB-8. **V** alterna primera / tercera persona. Ambos se mueven con WASD y saltan con espacio; Shift mantiene el turbo de BB-8.

Mantén **E**, habla en español y suelta para que BB-8 reaccione. La primera vez, permite el micrófono en el diálogo de macOS; si soltaste E durante ese diálogo, vuelve a mantenerlo. Máximo 15 segundos por intervención. El sonido de la escena se silencia durante la grabación para evitar que el propio droide se transcriba. Cambiar de aplicación cancela la grabación.

**T** abre una entrada de texto; Enter envía y Escape cancela. **F3** muestra la interpretación estimada, permite cambiar de micrófono y probar dos gestos. Esta información está oculta normalmente para no condicionar al participante.

El servicio de voz se inicia automáticamente. El motor está instalado en `~/Library/Application Support/BB8Voice` (aproximadamente 3,9 GB entre modelos y Python); `.local-voice` en el proyecto es un enlace a esa instalación. Así la voz no necesita acceder a Documentos al iniciar. Para actualizar el servicio tras editar `service.py`, ejecuta `.local-voice/venv/bin/python Assets/StreamingAssets/VoiceService/install.py`. El primer inicio puede tardar; espera a “Mantén E para hablar”. Todo funciona localmente una vez descargado: no hay suscripciones, claves externas ni grabaciones enviadas a Internet. No se guardan audio ni transcripciones; las últimas tres frases viven temporalmente en memoria para contexto y el registro técnico no incluye su contenido. El proceso que inicia la aplicación se cierra al salir.

La clasificación interpreta el significado del texto transcrito, **no el tono de voz ni el estado psicológico real**. Separa emoción expresada (`happy`, `sad`, `angry`, `fear`, `neutral`, `uncertain`) de actitud (`friendly`, `hostile`, `seeking_comfort`, `neutral`). No es una medición validada: las etiquetas e intensidad son estimaciones del modelo y pueden equivocarse, especialmente con ironía, ruido o transcripción incorrecta.

Los seis perfiles de movimiento y audio están en `Assets/Conversation/Reactions`: alegría con pequeño salto, consuelo con acercamiento suave, retirada ante hostilidad, cautela, atención y confusión. Una vez seleccionada una reacción, su secuencia es predefinida; la intensidad modula amplitud. La geometría puede limitar desplazamientos para evitar bordes y obstáculos. La reacción toma temporalmente el control de BB-8, incluso si lo estás manejando. El Minion sigue disponible para desplazarse.

Para reconstruir: **BB8 > Build Mac App**. Para verificar: **BB8 > Verify Conversation**, **BB8 > Verify Scrapyard**, y `.local-voice/venv/bin/python Assets/StreamingAssets/VoiceService/verify.py` con el servicio activo. Informes en `Logs/`. **Install Conversation** reinstala los objetos de conversación de la escena; conserva perfiles existentes.

Créditos del Minion y modelos locales: `THIRD-PARTY.md`.
