"""Conservative speech conditioning; audio stays in memory on the local Mac."""
from math import gcd
import numpy as np
from scipy.signal import butter, sosfilt, resample_poly


def prepare_audio(audio, sample_rate):
    if sample_rate < 8000 or sample_rate > 192000:
        raise ValueError('Frecuencia de micrófono no compatible.')
    if len(audio) > sample_rate * 16:
        raise ValueError('Máximo 15 segundos por intervención.')
    if len(audio) < sample_rate * .3:
        raise ValueError('Habla durante al menos un segundo.')
    if not np.isfinite(audio).all():
        raise ValueError('La grabación contiene audio inválido.')
    audio = np.asarray(audio, dtype=np.float32)
    if audio.ndim == 2:
        audio = audio.mean(axis=1)
    audio = audio - np.mean(audio)
    # Remove DC and low rumble, not speech frequencies. No generative denoising.
    audio = sosfilt(butter(2, 70, btype='highpass', fs=sample_rate, output='sos'), audio)
    rms = float(np.sqrt(np.mean(audio ** 2)))
    if rms < .0008:
        raise ValueError('La voz llegó demasiado baja. Revisa que el micrófono del visor no esté tapado e inténtalo nuevamente.')
    # Bounded gain helps quiet capture, without clipping or boosting silence.
    gain = min(6., .045 / rms, .94 / max(float(np.max(np.abs(audio))), .001))
    audio *= gain
    if sample_rate != 16000:
        divisor = gcd(sample_rate, 16000)
        audio = resample_poly(audio, 16000 // divisor, sample_rate // divisor)
    return audio.astype(np.float32)
