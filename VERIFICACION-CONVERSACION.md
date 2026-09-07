# Verificación local

Equipo: Apple M5, macOS, Unity 6000.5.8f1. Implementación gratuita local.

Actualización de expresividad: 13/13 comprobaciones nuevas. Retirada física medida a intensidades 1/2/3: 0,49 / 1,86 / 3,16 m en la zona de prueba; inclinación de cabeza medida de 7° a 24°. También se verificaron 18 grabaciones distintas, recorrido lateral de alegría, evolución de la barra, palabra del estado de BB-8, desaparición del popup, detención ante un obstáculo y giro automático de cámara sin mouse. Informe: `Logs/expression-verification.txt`.

Clasificador actualizado: 10/10 casos pasaron, incluidas frases de felicidad suave, normal y extrema con intensidades 1, 2 y 3 respectivamente; se conservaron los casos de negación, miedo, tristeza, hostilidad y cambio de emoción. Informe: `Logs/intensity-verification.json`. La app compiló correctamente y se revisó visualmente el popup con onomatopeya, estado y barra durante una interacción en la aplicación independiente.

- 14/14 comprobaciones de integración en Unity: referencias y huesos del Minion, control inicial, cambio de personaje/cámara, ambas perspectivas, conexión al servicio, clasificación conectada a reacción física, suspensión del control manual durante gestos, fin del gesto, retirada física y acotada, permanencia en el mapa y seis perfiles con audio.
- 17/17 comprobaciones del desguace: física, salto, antenas, impactos, turbo, límite de velocidad, descarga y recarga de batería, rampas y cruce del hueco.
- 10/10 comprobaciones del servicio: ocho frases en español (incluidas negación y cambio de emoción), rechazo de silencio y exigencia del token local.
- Audio sintético en español: “Hola BB ocho. Estoy muy feliz de verte. Hoy ha sido un día maravilloso.” → transcripción semánticamente equivalente → `happy / friendly / celebrate`, 1,25 segundos de procesamiento con modelos cargados.
- Aplicación macOS compilada; inicio automático del proceso Python y estado `ready` verificados sin iniciar manualmente el servicio. La primera carga tarda más que una intervención posterior. El motor se instaló después en Application Support para evitar depender del acceso a Documentos.

Los informes detallados están en `Logs/` (archivos locales, excluidos de Git). Esta batería pequeña comprueba funcionamiento, no demuestra precisión general de clasificación emocional ni sustituye la evaluación con participantes. El audio sintético comprueba STT y clasificación. El usuario confirmó que el micrófono físico funciona tras resolver el permiso de macOS.
