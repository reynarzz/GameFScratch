using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    internal class AudioAssetProcessor : IAssetProcessor
    {
        byte[] IAssetProcessor.Process(string path)
        {
            using var reader = new AudioFileReader(path);
            var format = reader.WaveFormat;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // Write header (leave PCM length for now)
            bw.Write(format.SampleRate);
            bw.Write(format.BitsPerSample); // 32-bit float
            bw.Write(format.Channels);
            bw.Write(reader.TotalTime.TotalSeconds);
            long pcmLengthPosition = ms.Position;
            bw.Write(0L); // placeholder for PCM length

            // Write PCM data and count bytes
            var buffer = new byte[format.AverageBytesPerSecond];
            int bytesRead;
            long totalPCMBytes = 0;

            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                bw.Write(buffer, 0, bytesRead);
                totalPCMBytes += bytesRead;
            }

            // Go back and write the actual PCM length
            long endPosition = ms.Position;
            ms.Position = pcmLengthPosition;
            bw.Write(totalPCMBytes);
            ms.Position = endPosition;

            return ms.ToArray();
        }

    }
}