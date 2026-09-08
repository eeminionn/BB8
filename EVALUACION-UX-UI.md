# BB–8 · Revisión de experiencia y presentación

8 de septiembre de 2026. Evaluación del prototipo local de interacción, realizada sobre la escena, controladores, geometría del Minion y aplicación de escritorio. No es un estudio con participantes.

## Hallazgos y cambios

| Problema observado | Efecto en la experiencia | Corrección |
|---|---|---|
| Pies del Minion separados de la base del controlador; margen de colisión de 8 cm | Sensación de estar volando y animación sin peso | Alineación mediante vértices y pesos reales del esqueleto; altura visible de 1,70 m; margen de 2,5 cm y detección de suelo estable |
| Movimiento instantáneo; animaciones usando el mismo eje local en huesos con orientaciones distintas | Deslizamiento y gestos poco naturales | Aceleración/frenado, marcha basada en velocidad real y ejes de articulación corregidos; reposo, salto, caída, aterrizaje, gesto al hablar y saludo con H |
| Dos HUD con estilos y controles repetidos; batería visible al controlar al Minion | Ruido visual y poca jerarquía | Una sola interfaz con estado de personaje y voz arriba, interacción abajo y batería contextual de BB–8 |
| Cámara movida por cualquier desplazamiento del mouse, inercia y variación del campo de visión | Giros involuntarios y encuadre inestable | Mirada con botón derecho, seguimiento del giro, campo de visión fijo de 65°, amortiguación de seguimiento y colisión con el escenario |
| BB–8 empezaba detrás del Minion | La interacción principal quedaba fuera del encuadre | Llegada orientada hacia BB–8 y pequeña zona de encuentro señalada con placas |
| Materiales saturados, reflejos fuertes e interfaz poco relacionada con el desguace | Apariencia de elementos ensamblados por separado | Paleta petróleo/cian/arena, iluminación menos naranja, reflejos moderados y componentes de interfaz compartidos |
| Inicio directo sin presentación ni controles de sonido | Falta de contexto y audio inesperado | Bienvenida, menú de pausa con Esc, controles resumidos y sonido desactivado al iniciar |
| Popup demasiado grande y separado del cuerpo | Leía como una interfaz flotante ajena al personaje | Tamaño reducido, anclaje más próximo a BB–8, oclusión por objetos y ocultación durante el menú |

## Decisiones de alcance

Se mantiene una experiencia de exploración y conversación, sin objetivos, puntuaciones ni economía adicional. Las animaciones del Minion son procedurales sobre su esqueleto actual; no se añadieron descargas ni dependencias de pago. El sonido puede activarse manualmente en el menú, pero inicia desactivado en cada apertura.

El texto sigue usando T para abrir, Enter para enviar y Esc para cerrar. Las letras t/T se escriben normalmente. Esc fuera del campo abre el menú y pausa la simulación. «Sígueme» y «aléjate» conservan su función.

## Preparación para VR

El popup ya ocupa una posición real en el escenario. Se eliminaron cambios automáticos del campo de visión y sacudidas de cámara. El controlador de cámara de escritorio deja de imponer su transformación si XR está activo.

Esto **no constituye todavía una versión VR**. Para esa etapa se requiere un rig XR, entrada de mandos, menús espaciales accesibles, ajuste a escala y altura del usuario, interacción con rayos/manos y pruebas con un visor real. Conviene ofrecer teletransporte y giro por pasos; el seguimiento automático de cámara de escritorio no debe trasladarse a la cabeza del usuario. La interfaz de escritorio sigue siendo IMGUI y deberá tener un presentador espacial equivalente.

## Validación

15 comprobaciones de presentación y locomoción: arranque silencioso, contacto estable, pies visibles a nivel del suelo, pausa, desplazamiento, articulaciones, frenado, salto/aterrizaje, saludo, habla, seguimiento de cámara, FOV fijo, cambio de primera persona, popup 3D y silencio durante reacciones. Informe reproducible: `Logs/presentation-verification.txt`.

La geometría inicial dejaba los pies 7,3 cm sobre la base del controlador, además del margen de colisión. Tras corregirla, la comprobación de vértices deformados mide 0,000 m sobre el suelo en reposo.

También pasaron las 13 comprobaciones de expresiones y las 8 de órdenes de movimiento: **36 comprobaciones en total**. La aplicación se compiló para macOS y se revisaron visualmente la bienvenida, el encuadre inicial, la interfaz y el apoyo del Minion, manteniendo el sonido desactivado.

Las comprobaciones automáticas no sustituyen una prueba de comodidad prolongada con usuarios ni garantizan ausencia de cualquier defecto en todos los rincones del mapa. No hay navegación global por laberintos ni IK de pies para cada irregularidad del terreno; el seguimiento de BB–8 conserva sus desvíos locales y protección de bordes.
