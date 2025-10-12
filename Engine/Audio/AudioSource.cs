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

        private static WaveOutEvent _sharedOutput;
        private static MixingSampleProvider _mixer;


        private bool _loop;
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                RebuildSampleProvider();
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

                    RebuildSampleProvider();
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

        static AudioSource()
        {
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true
            };
            _sharedOutput = new WaveOutEvent()
            {
                DesiredLatency = 70, // This should come from a config file
                NumberOfBuffers = 2,
            };
            _sharedOutput.Init(_mixer);
            _sharedOutput.Play();
        }

        internal override void OnInitialize()
        {
            base.OnInitialize();
            _output = new WaveOutEvent();
        }


        private void RebuildSampleProvider()
        {
            if (_audioClip == null || _audioClip.RawPCM.Length == 0 || _volumeProvider == null)
                return;

            ISampleProvider baseProvider;

            if (_audioClip.Channels == 1)
            {
                if (_panProvider == null)
                    _panProvider = new PanningSampleProvider(_volumeProvider) { Pan = 0f };

                baseProvider = _panProvider;
            }
            else
            {
                baseProvider = _volumeProvider;
            }

            _sampleProviderToPlay = Loop ? new LoopingSampleProvider(baseProvider) : baseProvider;
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

        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            if (clip == null || clip.RawPCM.Length == 0)
                return;

            // Convert clip format to mixer format if needed
            var clipFormat = (clip.BitsPerSample == 32)
                ? WaveFormat.CreateIeeeFloatWaveFormat(clip.SampleRate, clip.Channels)
                : new WaveFormat(clip.SampleRate, clip.BitsPerSample, clip.Channels);

            var provider = new BufferedWaveProvider(clipFormat);
            provider.AddSamples(clip.RawPCM, 0, clip.RawPCM.Length);

            ISampleProvider sampleProvider = provider.ToSampleProvider();

            // Resample if the sample rate doesn't match the mixer's
            if (clipFormat.SampleRate != _mixer.WaveFormat.SampleRate)
            {
                sampleProvider = new WdlResamplingSampleProvider(sampleProvider, _mixer.WaveFormat.SampleRate);
            }

            // Convert mono to stereo if needed
            if (clipFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
            {
                sampleProvider = new MonoToStereoSampleProvider(sampleProvider);
            }

            // Apply volume
            sampleProvider = new VolumeSampleProvider(sampleProvider) { Volume = volume };

            _mixer.AddMixerInput(sampleProvider);
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

            _output.Init(GetReverseProvider());
            _output.Play();
        }

        private ISampleProvider GetReverseProvider()
        {
            if (_reverseStream == null) return null;

            ISampleProvider provider = _reverseStream.ToSampleProvider();
            if (Loop)
                provider = new LoopingReverseWaveStream(_reverseStream).ToSampleProvider();
            return provider;
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

        public override TimeSpan CurrentTime
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

    public class LoopingSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;

        public LoopingSampleProvider(ISampleProvider source)
        {
            _source = source;
            WaveFormat = source.WaveFormat;
        }

        public WaveFormat WaveFormat { get; }

        public int Read(float[] buffer, int offset, int count)
        {
            int totalRead = 0;

            while (totalRead < count)
            {
                int read = _source.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0) // reached end of source
                {
                    if (_source is WaveStream ws)
                    {
                        ws.Position = 0; // rewind
                    }
                    else
                    {
                        break; // cannot loop, stop reading
                    }
                }
                totalRead += read;
            }

            return totalRead;
        }
    }

    public class LoopingReverseWaveStream : WaveStream
    {
        private readonly ReverseWaveStream _reverseStream;

        public LoopingReverseWaveStream(ReverseWaveStream reverseStream)
        {
            _reverseStream = reverseStream ?? throw new ArgumentNullException(nameof(reverseStream));
        }

        public override WaveFormat WaveFormat => _reverseStream.WaveFormat;
        public override long Length => _reverseStream.Length;

        public override long Position
        {
            get => _reverseStream.Position;
            set => _reverseStream.Position = value;
        }

        public override TimeSpan CurrentTime
        {
            get => _reverseStream.CurrentTime;
            set => _reverseStream.CurrentTime = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _reverseStream.Read(buffer, offset, count);

            if (bytesRead == 0 && _reverseStream.Length > 0) // reached start
            {
                _reverseStream.Position = 0; // rewind to end for looping
                bytesRead = _reverseStream.Read(buffer, offset, count);
            }

            return bytesRead;
        }
    }

}
