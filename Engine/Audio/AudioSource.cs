using Engine.Layers;
using GlmNet;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;
using System;

namespace Engine
{
    public class AudioSource : Component
    {
        private AudioClip _audioClip;
        private SoundPlayer _soundPlayer;
        private RawDataProvider _provider;

        public AudioClip Clip
        {
            get => _audioClip;
            set
            {
                if (_audioClip == value)
                {
                    return;
                }

                _audioClip = value;

                if (_audioClip)
                {
                    _provider = new RawDataProvider(_audioClip.RawAudioData);
                    _soundPlayer = AudioLayer.GetSoundPlayer(GetFormatFromClip(_audioClip), _provider);
                }
                else if (_soundPlayer != null)
                {
                    _soundPlayer.Parent.RemoveComponent(_soundPlayer);
                    _soundPlayer.Dispose();
                    _soundPlayer = null;
                    _provider.Dispose();
                }
            }
        }

        public bool Loop
        {
            get => _soundPlayer?.IsLooping ?? false;
            set
            {
                if (_soundPlayer == null)
                    return;

                _soundPlayer.IsLooping = value;
            }
        }

        public float Volume
        {
            get => _soundPlayer?.Volume ?? -1;
            set
            {
                if (_soundPlayer == null)
                    return;

                _soundPlayer.Volume = value;
            }
        }

        public float Pan
        {
            get => _soundPlayer?.Pan ?? -1;
            set
            {
                if (_soundPlayer == null)
                    return;

                _soundPlayer.Pan = value;
            }
        }

        public float PlaybackSpeed
        {
            get => _soundPlayer?.PlaybackSpeed ?? -1;
            set
            {
                if (_soundPlayer == null)
                    return;

                _soundPlayer.PlaybackSpeed = value;
            }
        }

        public float Time
        {
            get => _soundPlayer?.Time ?? -1;
            set
            {
                if (_soundPlayer == null)
                    return;

                _soundPlayer.Seek(value);
            }
        }

        public bool IsPlaying => _soundPlayer?.State == PlaybackState.Playing;
        public bool IsPaused => _soundPlayer?.State == PlaybackState.Paused;
        public bool IsStopped => _soundPlayer?.State == PlaybackState.Stopped;

        public void Play()
        {
            if (_soundPlayer == null)
                return;

            _soundPlayer.Play();
        }

        public void PlayAt(float seconds)
        {
            if (_soundPlayer == null)
                return;

            Time = seconds;
            Play();
        }

        public void PlayOneShot(AudioClip clip)
        {
            PlayOneShot(clip, 1.0f);
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f)
        {
            var provider = new RawDataProvider(clip.RawAudioData);
            var sound = AudioLayer.GetSoundPlayer(GetFormatFromClip(clip), provider);
            
            void OnEnded(object sender, EventArgs e)
            {
                sound.PlaybackEnded -= OnEnded;
                sound.Parent.RemoveComponent(sound);
                sound.Dispose();
                provider.Dispose();
            }

            sound.PlaybackEnded += OnEnded;
            sound.Volume = volume;
            sound.Play();
        }

        public void Pause()
        {
            if (_soundPlayer == null)
                return;

            _soundPlayer.Pause();
        }

        public void Stop()
        {
            if (_soundPlayer == null)
                return;

            _soundPlayer.Stop();
        }

        private AudioFormat GetFormatFromClip(AudioClip clip)
        {
            return new AudioFormat() { Channels = clip.Channels, Format = (SampleFormat)clip.SampleFormat, SampleRate = clip.SampleRate };
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _soundPlayer.Dispose();
            _provider.Dispose();
        }
    }
}