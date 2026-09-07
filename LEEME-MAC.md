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
