# Menú galáctico y destinos — 9 de septiembre de 2026

Se conservaron los controles y la conversación local. Se rediseñó el menú siguiendo las referencias del usuario: fondo estelar, título dorado delineado, tipografía condensada, marcos angulares de consola y planetas de navegación. El menú usa elementos propios del proyecto; los continentes de Tierra proceden de Natural Earth y las tipografías tienen licencia OFL.

Los destinos son **Khepra — Puesto 08** y **Tierra — Distrito Central**. Tierra es un barrio estilizado y transitable, con plaza, cruces peatonales, comercios, edificios, parada de bus, bancos, árboles, autos estacionados y dos puntos de recarga. Los comercios son exteriores; no hay tráfico ni peatones simulados.

El cambio de mapa conserva las instancias de los personajes, el servicio de voz y la afinidad. Cancela movimientos anteriores, oculta el popup viejo y coloca a los personajes sobre suelo seguro. Cambiar de destino durante el procesamiento de una frase espera a que este termine. Volver al destino activo reanuda sin reiniciar la posición.

**18 comprobaciones automáticas superadas**, registradas en `Logs/destination-verification.txt`: destino inicial, fuentes incluidas, activación exclusiva de entornos, cancelación de movimientos, suelo para ambos personajes, ausencia de duplicados, servicio de voz conservado, recarga, pertenencia del suelo al mapa activo, desplazamiento, retorno a Khepra, segundo viaje a Tierra, reutilización del entorno, afinidad, primera persona, silencio e índice inválido.

Se compiló la aplicación macOS y se revisaron visualmente el selector, el globo terrestre, la plaza urbana, los rótulos, el punto de llegada y el contraste del HUD. Todas las pruebas se realizaron con sonido desactivado.

La aplicación abre el menú con sonido apagado. Flechas izquierda/derecha o clic seleccionan destino, Enter viaja y Esc vuelve al entorno actual sin aplicar una selección pendiente. La integración de visor y mandos VR sigue fuera de esta entrega.

## Giro de los planetas

Los globos se renderizan en una textura de 512 × 512 mediante un shader que gira la superficie alrededor del eje norte-sur, con iluminación y silueta fijas. El reloj es independiente de la pausa; la vuelta dura 60 segundos. El recurso gráfico se reutiliza entre destinos y se libera al cerrar la interfaz. Tierra muestra la descripción «Planeta Tierra».

`BB8 > Verify Rotating Planets` compara imágenes renderizadas a 0, 15 y 60 segundos con `Time.timeScale = 0`. Para ambos destinos se comprobó cambio visible de superficie, silueta invariable, vuelta completa sin salto y transparencia correcta. Resultado en `Logs/planet-preview-verification.txt`. Las pruebas se ejecutaron con audio desactivado.

La compilación macOS se revisó con ambos destinos seleccionados y el menú abierto durante varias vueltas: continentes y terreno giran, la interfaz conserva su composición y la descripción de Tierra es breve. La previsualización se actualiza antes del dibujo de la interfaz y solo cuando el menú está abierto.
