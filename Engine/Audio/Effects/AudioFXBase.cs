using SoundFlow.Abstracts;
using SoundFlow.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class AudioFXBase : EObject
    {
        internal abstract SoundModifier InternalModifier { get; }
        public bool Enabled { get => InternalModifier.Enabled; set => InternalModifier.Enabled = value; }
        public AudioFXBase(AudioFormat format, string effectName) : base(effectName) { }
    }
}