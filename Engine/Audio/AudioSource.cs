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

        private bool _isReverse = false;
        private ReverseWaveStream _reverseStream;

        public bool Loop { get; set; } = false;

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
                _reverseStream = null;
                _isReverse = false;

                if (_audioClip != null && _audioClip.RawPCM.Length > 0)
                {
                    var format = (_audioClip.BitsPerSample == 32)
                        ? WaveFormat.CreateIeeeFloatWaveFormat(_audioClip.SampleRate, _audioClip.Channels)
                        : new WaveFormat(_audioClip.SampleRate, _audioClip.BitsPerSample, _audioClip.Channels);

                    // Streaming buffered provider for normal playback
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
                        _panProvider = new PanningSampleProvider(_volumeProvider)
                        {
                            Pan = 0f
                        };
                        _sampleProviderToPlay = _panProvider;
                    }
                    else
                    {
                        _sampleProviderToPlay = _volumeProvider;
                    }
                }
            }
        }

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

        public bool IsPlaying => _output?.PlaybackState == PlaybackState.Playing;
        public bool IsPaused => _output?.PlaybackState == PlaybackState.Paused;

        public float Time
        {
            get
            {
                if (_isReverse && _reverseStream != null)
                    return (float)_reverseStream.CurrentTime.TotalSeconds;
                if (!_isReverse && _bufferedProvider != null)
                    return (float)(_bufferedProvider.BufferLength - _bufferedProvider.BufferedBytes) /
                           _bufferedProvider.WaveFormat.AverageBytesPerSecond;
                return 0f;
            }
            set
            {
                if (_audioClip == null) return;

                if (_isReverse && _reverseStream != null)
                {
                    _reverseStream.CurrentTime = TimeSpan.FromSeconds(Math.Clamp(value, 0, _audioClip.Duration));
                }
                else if (!_isReverse && _bufferedProvider != null)
                {
                    long bytePos = (long)(value * _bufferedProvider.WaveFormat.AverageBytesPerSecond);
                    bytePos -= bytePos % _bufferedProvider.WaveFormat.BlockAlign;
                    bytePos = Math.Clamp(bytePos, 0, _audioClip.RawPCM.Length - _bufferedProvider.WaveFormat.BlockAlign);
                    _bufferedProvider.ClearBuffer();
                    _bufferedProvider.AddSamples(_audioClip.RawPCM, (int)bytePos, _audioClip.RawPCM.Length - (int)bytePos);
                }
            }
        }

        internal override void OnInitialize()
        {
            base.OnInitialize();
            _output = new WaveOutEvent();
            _output.PlaybackStopped += OnPlaybackStopped;
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (Loop && _audioClip != null)
            {
                if (_isReverse && _reverseStream != null)
                {
                    _reverseStream.Position = 0;
                    _output.Init(_reverseStream.ToSampleProvider());
                    _output.Play();
                }
                else if (!_isReverse && _sampleProviderToPlay != null)
                {
                    _bufferedProvider.ClearBuffer();
                    _bufferedProvider.AddSamples(_audioClip.RawPCM, 0, _audioClip.RawPCM.Length);
                    _output.Init(_sampleProviderToPlay);
                    _output.Play();
                }
            }
        }

        public void Play()
        {
            if (_sampleProviderToPlay == null)
            {
                Debug.Error("Audio clip not set before playing.");
                return;
            }

            _isReverse = false;
            _bufferedProvider.ClearBuffer();
            _bufferedProvider.AddSamples(_audioClip.RawPCM, 0, _audioClip.RawPCM.Length);

            _output.Init(_sampleProviderToPlay);
            _output.Play();
        }

        public void PlayAt(float seconds)
        {
            Play();
            Time = seconds;
        }

        public void PlayReverse()
        {
            PlayReverseAt(0f);
        }

        public void PlayOneShot(AudioClip clip)
        {
            PlayOneShot(clip, 1.0f);
        }

        public void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null || clip.RawPCM.Length == 0)
                return;

            var format = (clip.BitsPerSample == 32)
                ? WaveFormat.CreateIeeeFloatWaveFormat(clip.SampleRate, clip.Channels)
                : new WaveFormat(clip.SampleRate, clip.BitsPerSample, clip.Channels);

            var provider = new BufferedWaveProvider(format)
            {
                BufferLength = clip.RawPCM.Length,
                DiscardOnBufferOverflow = true
            };
            provider.AddSamples(clip.RawPCM, 0, clip.RawPCM.Length);

            var volumeProvider = new VolumeSampleProvider(provider.ToSampleProvider())
            {
                Volume = Math.Clamp(volume, 0.0f, 1.0f)
            };

            var output = new WaveOutEvent();
            output.Init(volumeProvider);
            output.Play();

            // Dispose automatically when finished
            output.PlaybackStopped += (s, e) =>
            {
                output.Dispose();
            };
        }

        public void PlayReverseAt(float seconds)
        {
            if (_audioClip == null || _audioClip.RawPCM.Length == 0)
            {
                Debug.Error("No PCM data to reverse.");
                return;
            }

            _isReverse = true;

            var format = (_audioClip.BitsPerSample == 32)
                ? WaveFormat.CreateIeeeFloatWaveFormat(_audioClip.SampleRate, _audioClip.Channels)
                : new WaveFormat(_audioClip.SampleRate, _audioClip.BitsPerSample, _audioClip.Channels);

            _reverseStream = new ReverseWaveStream(_audioClip.RawPCM, format);
            _reverseStream.CurrentTime = TimeSpan.FromSeconds(seconds);

            _output.Init(_reverseStream.ToSampleProvider());
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
            _reverseStream = null;
            base.OnDestroy();
        }
    }

    public class ReverseWaveStream : WaveStream
    {
        private readonly byte[] _data;
        private readonly WaveFormat _format;
        private readonly int _frameSize;
        private long _framePosition;

        public ReverseWaveStream(byte[] data, WaveFormat format)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _format = format ?? throw new ArgumentNullException(nameof(format));
            _frameSize = format.BlockAlign;
            _framePosition = 0;
        }

        public override WaveFormat WaveFormat => _format;
        public override long Length => _data.Length;
        public override long Position
        {
            get => _framePosition * _frameSize;
            set => _framePosition = Math.Clamp(value / _frameSize, 0, Length / _frameSize);
        }

        public TimeSpan CurrentTime
        {
            get => TimeSpan.FromSeconds((_data.Length / _frameSize - _framePosition) / (double)_format.SampleRate);
            set
            {
                long frame = (long)((_data.Length / _frameSize) - value.TotalSeconds * _format.SampleRate);
                _framePosition = Math.Clamp(frame, 0, _data.Length / _frameSize);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int framesRequested = count / _frameSize;
            int framesAvailable = (int)(_data.Length / _frameSize - _framePosition);
            int framesToRead = Math.Min(framesRequested, framesAvailable);

            for (int i = 0; i < framesToRead; i++)
            {
                long srcIndex = (_framePosition + i) * _frameSize;
                long destIndex = offset + (framesToRead - 1 - i) * _frameSize;
                Buffer.BlockCopy(_data, (int)srcIndex, buffer, (int)destIndex, _frameSize);
            }

            _framePosition += framesToRead;
            return framesToRead * _frameSize;
        }
    }



    public static class WaveStreamExtensions
    {
        public static ISampleProvider ToSampleProvider(this WaveStream stream)
        {
            return new RawSourceWaveStreamProvider(stream);
        }
    }

    public class RawSourceWaveStreamProvider : ISampleProvider
    {
        private readonly WaveStream _stream;
        private readonly WaveFormat _format;

        public RawSourceWaveStreamProvider(WaveStream stream)
        {
            _stream = stream;
            _format = stream.WaveFormat;
        }

        public WaveFormat WaveFormat => _format;

        public int Read(float[] buffer, int offset, int count)
        {
            int bytesRequested = count * 4; // float = 4 bytes
            byte[] temp = new byte[bytesRequested];
            int bytesRead = _stream.Read(temp, 0, bytesRequested);

            // convert bytes to float
            int floatsRead = bytesRead / 4;
            Buffer.BlockCopy(temp, 0, buffer, offset * 4, floatsRead * 4);
            return floatsRead;
        }
    }
}
