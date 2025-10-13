using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Layers;
using SoundFlow.Abstracts;
using SoundFlow.Components;
using SoundFlow.Modifiers;

namespace Engine
{
    public class AudioMixer : EObject
    {
        private Mixer _internalMixer;
        public bool Enabled { get => _internalMixer.Enabled; set => _internalMixer.Enabled = value; }
        public float Pan { get => _internalMixer.Pan; set => _internalMixer.Pan = value; }
        public float Volume { get => _internalMixer.Volume; set => _internalMixer.Volume = value; }
        public bool Solo { get => _internalMixer.Solo; set => _internalMixer.Solo = value; }
        public bool Mute { get => _internalMixer.Mute; set => _internalMixer.Mute = value; }
        public override string Name { get => _internalMixer?.Name ?? base.Name; set => _internalMixer.Name = base.Name = value; }
        public bool IsMaster => _internalMixer.IsMasterMixer;

        public static AudioMixer Master => AudioLayer.GetMasterAudioMixer();

        public AudioMixer(string name) : this(name, AudioLayer.CreateMixer())
        {
        }

        internal AudioMixer(string name, Mixer mixer)
        {
            _internalMixer = mixer;
            Name = name;
        }

        internal void AddPlayer(SoundPlayer sound)
        {
            if (sound == null)
                return;

            if(_internalMixer == sound.Parent)
            {
                return;
            }

            sound.Parent?.RemoveComponent(sound);
            _internalMixer.AddComponent(sound);
        }

        internal void RemoveSource(AudioSource source)
        {
            _internalMixer.RemoveComponent(source.SoundPlayer);
        }

        public T AddAudioFX<T>() where T : AudioFXBase
        {
            var modifier = (T)Activator.CreateInstance(typeof(T), _internalMixer.Format);
            _internalMixer.AddModifier(modifier.InternalModifier);
            return modifier;
        }

        public void RemoveAudioFX(AudioFXBase fx)
        {
            _internalMixer.RemoveModifier(fx.InternalModifier);
        }

        public override void OnDestroy()
        {
            if (_internalMixer == null)
                return;
            Debug.Log("Destroy mixer: " + Name);
            _internalMixer.Parent.RemoveComponent(_internalMixer);
            _internalMixer.Dispose();
            _internalMixer = null;
        }

        ~AudioMixer()
        {
            OnDestroy();
        }
    }
}
