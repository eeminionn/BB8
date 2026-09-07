# BB8 en Mac

Proyecto actualizado para Unity 6000.5.8f1 y probado en Apple M5.

## Jugar en Unity

1. En Unity Hub, selecciona Add / Add project from disk y elige esta carpeta BB8.
2. Abre con Unity 6000.5.8f1.
3. Usa el menú BB8 > Open Demo (escena Assets/demo.unity).
4. Pulsa Play y haz clic dentro de la pestaña Game.

WASD mueve, el mouse orienta la cámara, espacio salta, la rueda ajusta el zoom y R reinicia la escena. El salto tiene una pausa de dos segundos entre usos.

## Jugar sin el editor

Abre BB8.app, situada junto a esta carpeta. Es una aplicación universal para Apple Silicon e Intel. Command-Q la cierra.

## Reconstruir

En el editor, usa BB8 > Build Mac App. Guarda la aplicación en ../BB8.app.
Blender 5.2.1 está instalado en /Applications/Blender.app y permite importar los tres modelos .blend originales.

## Cambios y verificación

- Actualización de Rigidbody.velocity a linearVelocity para Unity 6.
- Reparación de las referencias de las antenas después de importar con Blender moderno.
- Iluminación ambiental explícita sin reflejo ambiental defectuoso de la escena antigua.
- Escena de inicio, aplicación en ventana y controles visibles; R permite reiniciar.
- Dependencias limitadas a los módulos integrados del motor.
- Compilación de macOS correcta. Prueba del controlador con físicas reales: desplazamiento 12.06 m y salto 0.35 m. La prueba automática inyectó órdenes al controlador. También se verificaron teclado, salto y reinicio en la aplicación mediante su interfaz.

Origen: https://github.com/mattolenik/BB8
Fork: https://github.com/eeminionn/BB8
Rama: unity6-macos
