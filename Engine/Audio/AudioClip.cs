using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class AudioClip : EObject
    {
        internal byte[] RawPCM { get; }
        public double Duration { get; }
        public int SampleRate { get; }
        public int BitsPerSample { get; }
        public int Channels { get; }

        public AudioClip(string name, Guid id, byte[] rawPCM, double duration, int sampleRate, int bitsPerSample, int channels) : base(name, id)
        {
            RawPCM = rawPCM;
            Duration = duration;
            SampleRate = sampleRate;
            BitsPerSample = bitsPerSample;
            Channels = channels;
        }
    }
}
