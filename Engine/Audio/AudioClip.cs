using SoundFlow.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class AudioClip : EObject
    {
        public double Duration { get; }
        public int SampleRate { get; }
        public int Channels { get; }
        internal RawDataProvider Provider { get; }
        public AudioClip(string name, Guid id, float[] rawData, int sampleRate, int totalFrames, int channels) : base(name, id)
        {
            Duration = (float)totalFrames / sampleRate;
            SampleRate = sampleRate;
            Channels = channels;

            Provider = new RawDataProvider(rawData);
        }

        ~AudioClip()
        {
            Provider.Dispose();
        }
    }
}
