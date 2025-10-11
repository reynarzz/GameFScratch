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
            WaveStream reader;

            // Select the correct reader based on file extension
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".wav")
            {
                reader = new WaveFileReader(path); // gives true bits per sample
            }
            else
            {
                reader = new MediaFoundationReader(path); // MP3, AAC, WMA, FLAC
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            var format = reader.WaveFormat;

            // Write header
            bw.Write(format.SampleRate);
            bw.Write(format.BitsPerSample);
            bw.Write(format.Channels);
            bw.Write(reader.TotalTime.TotalSeconds);
            long pcmLengthPosition = ms.Position;
            bw.Write(0L); // placeholder for PCM length

            int bytesPerSample = format.BitsPerSample / 8;
            var buffer = new byte[format.AverageBytesPerSecond];
            int bytesRead;
            long totalPCMBytes = 0;

            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                bw.Write(buffer, 0, bytesRead);
                totalPCMBytes += bytesRead;
            }

            // Write the actual PCM length
            long endPosition = ms.Position;
            ms.Position = pcmLengthPosition;
            bw.Write(totalPCMBytes);
            ms.Position = endPosition;
            reader.Dispose();

            return ms.ToArray();
        }
    }

}