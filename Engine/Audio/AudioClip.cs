using SoundFlow.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class AudioClip : AssetResourceBase
    {
        public TimeSpan Duration { get; }
        public int SampleRate { get; }
        public int Channels { get; }

        internal int SampleFormat { get; }
        internal float[] RawAudioData { get; }
        
        public AudioClip(string path, Guid id, float[] rawData, int sampleRate, int totalSamples, int channels, int sampleFormat) : base(path, id)
        {
            Duration = TimeSpan.FromSeconds((float)totalSamples / (sampleRate * channels));
            SampleRate = sampleRate;
            Channels = channels;
            SampleFormat = sampleFormat;
            RawAudioData = rawData;
        }
    }
}
