using GlmNet;
using System;

namespace Engine
{
    public class AudioSource : Component
    {
        private AudioClip _audioClip;

        private bool _loop;
        public bool Loop
        {
            get => _loop;
            set => _loop = value;
        }

        public AudioClip Clip
        {
            get => _audioClip;
            set => _audioClip = value;
        }

        public float Volume { get; set; } = 1f;
        public float Pan { get; set; } = 0f;

        public bool IsPlaying => false;
        public bool IsPaused => false;

        public float Time { get; set; } = 0f;

        internal override void OnInitialize()
        {
            base.OnInitialize();
        }

        public void Play() { }
        public void PlayAt(float seconds) { Time = seconds; }
        public void PlayReverse() { }
        public void PlayReverseAt(float seconds) { Time = seconds; }

        public void PlayOneShot(AudioClip clip) { }
        public void PlayOneShot(AudioClip clip, float volume = 1f) { }

        public void Stop() { }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
