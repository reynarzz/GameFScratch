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
    public class CompressorAudioFX : AudioFXBase
    {
        private CompressorModifier _internalModifier;
        internal override SoundModifier InternalModifier => _internalModifier;

        /// <summary>
        /// The threshold level in dBFS (-inf to 0).
        /// </summary>
        public float ThresholdDb { get => _internalModifier.ThresholdDb; set => _internalModifier.ThresholdDb = value; }


        /// <summary>
        /// The compression ratio (1:1 to inf:1).
        /// </summary>
        public float Ratio { get => _internalModifier.Ratio; set => _internalModifier.Ratio = value; }


        /// <summary>
        /// The attack time in milliseconds.
        /// </summary>
        public float AttackMs { get => _internalModifier.AttackMs; set => _internalModifier.AttackMs = value; }

        /// <summary>
        /// The release time in milliseconds.
        /// </summary>
        public float ReleaseMs { get => _internalModifier.ReleaseMs; set => _internalModifier.ReleaseMs = value; }

        /// <summary>
        /// The knee radius in dBFS. A knee radius of 0 is a hard knee.
        /// </summary>
        public float KneeDb { get => _internalModifier.KneeDb; set => _internalModifier.KneeDb = value; }

        /// <summary>
        /// The make-up gain in dBFS.
        /// </summary>
        public float MakeupGainDb { get => _internalModifier.MakeupGainDb; set => _internalModifier.MakeupGainDb = value; }

        public CompressorAudioFX(AudioFormat format) : base(format, "Compressor FX")
        {
            _internalModifier = new CompressorModifier(format, 0, 1, 0, 1);
        }
    }

}
