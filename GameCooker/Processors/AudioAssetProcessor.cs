using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharedTypes;
using SoundFlow;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace GameCooker
{
    internal class AudioAssetProcessor : IAssetProcessor
    {
        private readonly static MiniAudioEngine _engine = new MiniAudioEngine();

        byte[] IAssetProcessor.Process(string path, AssetMetaFileBase meta)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            var audioMeta = (meta as AudioMetaFile);

            var format = new AudioFormat()
            {
                Channels = audioMeta.Config.Channels,
                SampleRate = audioMeta.Config.SampleRate,
                Format = (SampleFormat)audioMeta.Config.SampleFormat
            };

            using var provider = new StreamDataProvider(_engine, format, fs);

            float[] buffer = new float[provider.Length];

            int samplesData = provider.ReadBytes(buffer);
           
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(format.SampleRate);
            bw.Write(format.Channels);
            bw.Write((int)format.Format);
            bw.Write(samplesData);

            // Write all float samples
            foreach (var sample in buffer)
            {
                bw.Write(sample);
            }

            return ms.ToArray();
        }
    }
}