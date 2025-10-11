using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.IO
{
    internal class AudioClipAssetBuilder : AssetBuilderBase
    {
        internal override EObject BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            var sampleRate = reader.ReadInt32();
            var bitsPerSample = reader.ReadInt32();
            var channels = reader.ReadInt32();
            var duration = reader.ReadDouble();
            var pcmBytes = reader.ReadInt64();

            var data = new byte[pcmBytes];

            int bytesRead = 0;
            while (bytesRead < data.Length)
            {
                int read = reader.BaseStream.Read(data, bytesRead, data.Length - bytesRead);

                if (read == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream while reading texture data.");
                }
                bytesRead += read;
            }

            return new AudioClip(Path.GetFileNameWithoutExtension(info.Path), guid, data, duration, sampleRate, bitsPerSample, channels);
        }
    }
}
