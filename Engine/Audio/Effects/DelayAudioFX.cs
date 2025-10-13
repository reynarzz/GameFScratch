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
    public class DelayAudioFX : AudioFXBase
    {
        private DelayModifier _internalModifier;
        internal override SoundModifier InternalModifier => _internalModifier;

        /// <summary>
        /// The feedback amount (0.0 - 1.0).
        /// </summary>
        public float Feedback { get => _internalModifier.Feedback; set => _internalModifier.Feedback = value; }

        /// <summary>
        /// The wet/dry mix (0.0 - 1.0).
        /// </summary>
        public float WetMix { get => _internalModifier.WetMix; set => _internalModifier.WetMix = value; }

        /// <summary>
        /// The cutoff frequency in Hertz.
        /// </summary>
        public float Cutoff { get => _internalModifier.Cutoff; set => _internalModifier.Cutoff = value; }

        public DelayAudioFX(AudioFormat format) : base(format, "Delay FX")
        {
            _internalModifier = new DelayModifier(format);
        }
    }
}
