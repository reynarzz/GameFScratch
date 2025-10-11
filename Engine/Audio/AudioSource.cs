using Engine.Audio;
using GlmNet;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace Engine
{
    public class AudioSource : Component
    {
        private WaveOutEvent _output;
        private BufferedWaveProvider _bufferedProvider;
        private ISampleProvider _sampleProviderToPlay;
        private VolumeSampleProvider _volumeProvider;
        private PanningSampleProvider _panProvider;
        private AudioClip _audioClip;

        public float Volume
        {
            get => _volumeProvider?.Volume ?? 1f;
            set
            {
                if (_volumeProvider != null)
                    _volumeProvider.Volume = Math.Clamp(value, 0f, 1f);
            }
        }

        public float Pan
        {
            get => _panProvider?.Pan ?? 0f;
            set
            {
                if (_panProvider != null)
                    _panProvider.Pan = Math.Clamp(value, -1f, 1f);
            }
        }



        public AudioClip Clip
        {
            get => _audioClip;
            set
            {
                if (_audioClip == value) return;
                Stop();

                _audioClip = value;

                _bufferedProvider = null;
                _sampleProviderToPlay = null;
                _volumeProvider = null;
                _panProvider = null;

                if (_audioClip != null && _audioClip.RawPCM.Length > 0)
                {
                    WaveFormat format = (_audioClip.BitsPerSample == 32)
                        ? WaveFormat.CreateIeeeFloatWaveFormat(_audioClip.SampleRate, _audioClip.Channels)
                        : new WaveFormat(_audioClip.SampleRate, _audioClip.BitsPerSample, _audioClip.Channels);

                    _bufferedProvider = new BufferedWaveProvider(format)
                    {
                        BufferLength = _audioClip.RawPCM.Length,
                        DiscardOnBufferOverflow = true
                    };
                    _bufferedProvider.AddSamples(_audioClip.RawPCM, 0, _audioClip.RawPCM.Length);

                    _volumeProvider = new VolumeSampleProvider(_bufferedProvider.ToSampleProvider())
                    {
                        Volume = 1f
                    };

                    if (_audioClip.Channels == 1)
                    {
                        // Only mono supports PanningSampleProvider
                        _panProvider = new PanningSampleProvider(_volumeProvider)
                        {
                            Pan = 0f
                        };
                        _sampleProviderToPlay = _panProvider;
                    }
                    else
                    {
                        _sampleProviderToPlay = _volumeProvider; // stereo, no pan
                    }
                }
            }
        }

        internal override void OnInitialize()
        {
            base.OnInitialize();
            _output = new WaveOutEvent();
        }

        public void Play()
        {
            if (_sampleProviderToPlay == null)
            {
                Debug.Error("Audio clip not set before playing.");
                return;
            }

            _bufferedProvider.ClearBuffer();
            _bufferedProvider.AddSamples(_audioClip.RawPCM, 0, _audioClip.RawPCM.Length);

            _output.Init(_sampleProviderToPlay);
            _output.Play();
        }

        public void PlayAt(float seconds)
        {
            if (_sampleProviderToPlay == null)
            {
                Debug.Error("Audio clip not set before playing.");
                return;
            }

            WaveFormat format = _bufferedProvider.WaveFormat;
            long startByte = (long)(seconds * format.AverageBytesPerSecond);
            startByte -= startByte % format.BlockAlign;
            startByte = Math.Clamp(startByte, 0, _audioClip.RawPCM.Length - format.BlockAlign);

            int length = _audioClip.RawPCM.Length - (int)startByte;

            _bufferedProvider.ClearBuffer();
            _bufferedProvider.AddSamples(_audioClip.RawPCM, (int)startByte, length);

            _output.Init(_sampleProviderToPlay);
            _output.Play();
        }

        public void Apply3DAudio(vec3 listenerPos, vec3 sourcePos, float maxDistance = 20f)
        {
            if (_volumeProvider == null) return;

            vec3 diff = sourcePos - listenerPos;
            float distance = diff.length();

            float volume = Math.Clamp(1f - (distance / maxDistance), 0f, 1f);

            _volumeProvider.Volume = volume;

            if (_panProvider != null)
            {
                float pan = Math.Clamp(diff.x / maxDistance, -1f, 1f);
                _panProvider.Pan = pan;
            }
        }

        public void PlayReverse()
        {
            PlayReverseAt(0f);
        }

        public void PlayReverseAt(float seconds)
        {
            if (_audioClip == null || _audioClip.RawPCM.Length == 0)
            {
                Debug.Error("No PCM data to reverse.");
                return;
            }

            WaveFormat format = (_audioClip.BitsPerSample == 32)
                ? WaveFormat.CreateIeeeFloatWaveFormat(_audioClip.SampleRate, _audioClip.Channels)
                : new WaveFormat(_audioClip.SampleRate, _audioClip.BitsPerSample, _audioClip.Channels);

            int frameSize = format.BlockAlign;
            long startByte = (long)(seconds * format.AverageBytesPerSecond);
            startByte -= startByte % frameSize;
            startByte = Math.Clamp(startByte, 0, _audioClip.RawPCM.Length - frameSize);

            long totalFrames = (_audioClip.RawPCM.Length - startByte) / frameSize;
            int chunkFrames = 4096; // memory-efficient chunks
            byte[] chunkBuffer = new byte[chunkFrames * frameSize];

            _bufferedProvider = new BufferedWaveProvider(format)
            {
                BufferLength = (int)(_audioClip.RawPCM.Length - startByte),
                DiscardOnBufferOverflow = true
            };

            for (long frame = 0; frame < totalFrames; frame += chunkFrames)
            {
                int framesThisChunk = (int)Math.Min(chunkFrames, totalFrames - frame);
                int bytesThisChunk = framesThisChunk * frameSize;

                for (int i = 0; i < framesThisChunk; i++)
                {
                    long srcIndex = _audioClip.RawPCM.Length - frameSize - (frame + i) * frameSize;
                    Buffer.BlockCopy(_audioClip.RawPCM, (int)srcIndex, chunkBuffer, i * frameSize, frameSize);
                }

                _bufferedProvider.AddSamples(chunkBuffer, 0, bytesThisChunk);
            }

            _volumeProvider = new VolumeSampleProvider(_bufferedProvider.ToSampleProvider())
            {
                Volume = 1f
            };
             
            if (_audioClip.Channels == 1)
            {
                _panProvider = new PanningSampleProvider(_volumeProvider) { Pan = 0f };
                _sampleProviderToPlay = _panProvider;
            }
            else
            {
                _sampleProviderToPlay = _volumeProvider;
            }

            _output.Init(_sampleProviderToPlay);
            _output.Play();
        }

        public void Stop()
        {
            _output?.Stop();
        }

        public override void OnDestroy()
        {
            _output?.Dispose();
            _output = null;
            _bufferedProvider = null;
            _sampleProviderToPlay = null;
            _volumeProvider = null;
            _panProvider = null;
            base.OnDestroy();
        }
    }
}
