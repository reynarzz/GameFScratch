using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class AudioClip : EObject
    {
        internal float[] RawPCM { get; }
        public double Duration { get; }
        public int SampleRate { get; }
        public int Channels { get; }

        public AudioClip(string name, Guid id, float[] rawPCM, int sampleRate, int totalFrames, int channels) : base(name, id)
        {
            RawPCM = rawPCM;
            Duration = (float)totalFrames / sampleRate;
            SampleRate = sampleRate;
            Channels = channels;
        }
    }
}
