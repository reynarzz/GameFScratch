using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    [Serializable]
    public struct AudioConfig
    {
        public int SampleRate;
        public int Channels;
        public int SampleFormat;
        public static readonly AudioConfig Default = new AudioConfig() { Channels = 2, SampleFormat = 5, SampleRate = 48000 };
    }

    [Serializable]
    public class AudioMetaFile : AssetMetaFileBase
    {
        public AudioConfig Config { get; set; } = AudioConfig.Default;
    }
}
