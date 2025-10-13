using SoundFlow.Abstracts;
using SoundFlow.Modifiers;
using SoundFlow.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class ChorusAudioFX : AudioFXBase
    {
        private ChorusModifier _internalModifier;

        public float DepthMs { get => _internalModifier.DepthMs; set => _internalModifier.DepthMs = Math.Max(0.0f, value); }
        public float RateHz { get => _internalModifier.RateHz; set => _internalModifier.RateHz = Math.Max(0.0f, value); }
        //public float Feedback { get => _internalModifier.Feedback; set => _internalModifier.Feedback = value; }
        public float WetDryMix { get => _internalModifier.WetDryMix; set => _internalModifier.WetDryMix = Math.Clamp(value, 0.0f, 1.0f); }
        // public float MaxDelayMs { get => _internalModifier.MaxDelayMs; set => _internalModifier.MaxDelayMs = value; }

        internal override SoundModifier InternalModifier => _internalModifier;

        public ChorusAudioFX(AudioFormat format) : base(format, "Chorus FX")
        {
            _internalModifier = new ChorusModifier(format);
        }
    }
}