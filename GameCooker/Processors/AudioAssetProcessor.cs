using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SoundFlow;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace GameCooker
{
    internal class AudioAssetProcessor : IAssetProcessor
    {
        private readonly static MiniAudioEngine _engine = new MiniAudioEngine();
        private readonly AudioFormat _format = AudioFormat.DvdHq; // 48kHz, 32-bit float stereo

        byte[] IAssetProcessor.Process(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            using var provider = new StreamDataProvider(_engine, _format, fs);

            float[] buffer = new float[provider.Length];

            int framesRead = provider.ReadBytes(buffer);
           
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(_format.SampleRate);
            bw.Write(_format.Channels);
            bw.Write((int)_format.Format);
            bw.Write(framesRead);

            // Write all float samples
            foreach (var sample in buffer)
            {
                bw.Write(sample);
            }

            return ms.ToArray();
        }
    }
}