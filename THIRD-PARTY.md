# Créditos y licencias

## Minion

[Minion - Character - Rigging](https://sketchfab.com/3d-models/minion-character-rigging-aac9063a39824178be43acb672a2e5bf) por [Cyber_Graphic3D / Olek.Jedynak](https://sketchfab.com/Olek.Jedynak), licencia [Creative Commons Attribution 4.0](https://creativecommons.org/licenses/by/4.0/). Descargado desde Sketchfab con la cuenta del usuario. Adaptaciones: escala, orientación, materiales para Unity, controlador y animación procedural del esqueleto. No se modificó el archivo FBX original. Minions es un personaje de sus respectivos titulares; la licencia indicada es la publicada por el creador del modelo.

## Voz y clasificación locales

- Whisper small: pesos `mlx-community/whisper-small-mlx`, modelo Whisper bajo licencia MIT; implementación mlx-whisper MIT.
- Qwen3-4B-Instruct-2507: pesos `mlx-community/Qwen3-4B-Instruct-2507-4bit`, Apache 2.0; implementación mlx-lm MIT.
- Los pesos y las dependencias están en `.local-voice/`, excluidos de Git. Se conservan sus archivos de licencia distribuidos.
- Sin API de pago. Procesamiento en el Mac mediante MLX / Metal. El servicio escucha exclusivamente en 127.0.0.1 y exige un token local para procesar solicitudes.

El repositorio BB8 original conserva su licencia y créditos. Los sonidos nuevos del desguace son síntesis original creada para este proyecto.

## Grabaciones de BB-8 para reacciones

18 extractos completos del [fork eeminionn/bb8-sounds](https://github.com/eeminionn/bb8-sounds), procedente de [anddav87/bb8-sounds](https://github.com/anddav87/bb8-sounds), cuyo README los describe como efectos oficiales de BB-8 para Sphero. Se convirtieron de MP3 a WAV mono, se normalizó el volumen y se añadieron fundidos de 6 ms para evitar clics. La correspondencia exacta está en `Assets/Conversation/Audio/sources.json`. El repositorio no publica una licencia específica para estas grabaciones; conservan los derechos de sus titulares.

La búsqueda encontró un [catálogo de The Sounds Resource](https://sounds.spriters-resource.com/pc_computer/legostarwarstheskywalkersaga/asset/438626/) con nombres como HAPPYCHIRP y ANGRYCHIRP, pero no se pudo descargar. Las asignaciones de esta implementación son provisionales, según duración y contorno acústico, para acompañar gestos; no son etiquetas emocionales oficiales ni resultados de validación con participantes.
