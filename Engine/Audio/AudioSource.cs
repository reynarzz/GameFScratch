using Engine.Audio;
using GlmNet;
using NAudio.Wave;
using System;

namespace Engine
{
    public class AudioSource : Component
    {
        private WaveOutEvent _output;
        private WaveStream _waveStream;
        private WaveChannel32 _channel;
        private AudioClip _audioClip;

        public AudioClip Clip
        {
            get => _audioClip;
            set
            {
                if (_audioClip == value) return;
                Stop();

                _audioClip = value;

                _waveStream?.Dispose();
                _waveStream = null;
                _channel = null;

                if (_audioClip != null)
                {
                    WaveFormat format;
                    if (_audioClip.BitsPerSample == 32)
                        format = WaveFormat.CreateIeeeFloatWaveFormat(_audioClip.SampleRate, _audioClip.Channels);
                    else
                        format = new WaveFormat(_audioClip.SampleRate, _audioClip.BitsPerSample, _audioClip.Channels);

                    int frameSize = format.BlockAlign;
                    long alignedLength = _audioClip.RawPCM.Length - (_audioClip.RawPCM.Length % frameSize);

                    _waveStream = new RawSourceWaveStream(_audioClip.RawPCM, 0, (int)alignedLength, format);
                    _channel = new WaveChannel32(_waveStream);
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
            if (_channel == null)
            {
                Debug.Error("Audio clip not set before playing.");
                return;
            }

            _waveStream.Position = 0;
            _output.Init(_channel);
            _output.Play();
        }

        public void PlayAt(float seconds)
        {
            if (_channel == null)
            {
                Debug.Error("Audio clip not set before playing.");
                return;
            }

            if (_waveStream is AudioFileReader afr)
            {
                afr.CurrentTime = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                long startPosition = (long)(seconds * _waveStream.WaveFormat.AverageBytesPerSecond);
                long frameSize = _waveStream.WaveFormat.BlockAlign;
                startPosition -= startPosition % frameSize;
                startPosition = Math.Clamp(startPosition, 0, _waveStream.Length);
                _waveStream.Position = startPosition;
            }

            _output.Init(_channel);
            _output.Play();
        }

        public void Apply3DAudio(vec3 listenerPos, vec3 sourcePos, float maxDistance = 20f)
        {
            if (_channel == null) return;

            vec3 diff = sourcePos - listenerPos;
            float distance = diff.length();

            float volume = Math.Clamp(1f - (distance / maxDistance), 0f, 1f);
            float pan = Math.Clamp(diff.x / maxDistance, -1f, 1f);

            _channel.Volume = volume;
            _channel.Pan = pan;
        }

        public void PlayReverse()
        {
            PlayReverseAt(0f);
        }

        public void PlayReverseAt(float seconds)
        {
            if (_audioClip == null)
            {
                Debug.Error("Audio clip not set before playing.");
                return;
            }

            if (_audioClip.RawPCM.Length == 0)
            {
                Debug.Error("No PCM data to reverse.");
                return;
            }

            var format = (_audioClip.BitsPerSample == 32)
                ? WaveFormat.CreateIeeeFloatWaveFormat(_audioClip.SampleRate, _audioClip.Channels)
                : new WaveFormat(_audioClip.SampleRate, _audioClip.BitsPerSample, _audioClip.Channels);

            int frameSize = format.BlockAlign;
            long startByte = (long)(seconds * format.AverageBytesPerSecond);

            // Align to frame
            startByte -= startByte % frameSize;
            startByte = Math.Clamp(startByte, 0, _audioClip.RawPCM.Length - frameSize);

            long lengthToReverse = _audioClip.RawPCM.Length - startByte;
            var reversed = new byte[lengthToReverse];

            for (long i = 0; i < lengthToReverse; i += frameSize)
            {
                long destIndex = lengthToReverse - frameSize - i;
                Buffer.BlockCopy(_audioClip.RawPCM, (int)(startByte + i), reversed, (int)destIndex, frameSize);
            }

            _waveStream?.Dispose();
            _waveStream = new RawSourceWaveStream(reversed, 0, reversed.Length, format);
            _channel = new WaveChannel32(_waveStream);

            _output.Init(_channel);
            _output.Play();
        }

        public void Stop()
        {
            _output?.Stop();
        }

        public override void OnDestroy()
        {
            _waveStream?.Dispose();
            _output?.Dispose();
            _waveStream = null;
            _channel = null;
            _output = null;
            base.OnDestroy();
        }
    }
}
