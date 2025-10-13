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
    public class ReverbAudioFX : AudioFXBase
    {
        private AlgorithmicReverbModifier _internalModifier;
        internal override SoundModifier InternalModifier => _internalModifier;

        public float Wet { get => _internalModifier.Wet; set => _internalModifier.Wet = value; }
        public float Width { get => _internalModifier.Width; set => _internalModifier.Width = value; } 
        public float Mix { get => _internalModifier.Mix; set => _internalModifier.Mix = value; }
        public float PreDelay { get => _internalModifier.PreDelay; set => _internalModifier.PreDelay = value; }
        public float RoomSize { get => _internalModifier.RoomSize; set => _internalModifier.RoomSize = value; }

        public ReverbAudioFX(AudioFormat format) : base(format, "Reverb FX")
        {
            _internalModifier = new AlgorithmicReverbModifier(format);
        }
    }
}
