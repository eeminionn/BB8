"""Signal-level regression checks. No microphone access, playback, or network."""
from pathlib import Path
import sys
import unittest
import numpy as np
sys.path.insert(0,str(Path(__file__).resolve().parents[1]/'Assets/StreamingAssets/VoiceService'))
from audio_input import prepare_audio


class AudioInputChecks(unittest.TestCase):
    def test_quiet_stereo_and_resampling(self):
        for rate in (16000,44100,48000):
            t=np.arange(rate*2)/rate
            signal=.002*np.sin(2*np.pi*220*t)*(np.sin(2*np.pi*3*t)>.1)
            result=prepare_audio(np.column_stack([signal,signal]),rate)
            self.assertEqual(len(result),32000)
            self.assertEqual(result.dtype,np.float32)
            self.assertGreater(float(np.sqrt(np.mean(result**2))),.004)

    def test_silence_dc_and_invalid_samples(self):
        for signal in (np.zeros(48000),np.ones(48000)*.1,np.ones(48000)*np.nan):
            with self.assertRaises(ValueError):prepare_audio(signal,48000)

    def test_loud_capture_stays_below_clipping(self):
        signal=.99*np.sin(2*np.pi*220*np.arange(48000)/48000)
        self.assertLessEqual(float(np.max(np.abs(prepare_audio(signal,48000)))),.95)

    def test_duration_limits(self):
        for seconds in (.1,17):
            with self.assertRaises(ValueError):prepare_audio(np.ones(int(48000*seconds)),48000)


if __name__=='__main__':unittest.main()
