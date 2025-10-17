using SharedTypes;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Modifiers;
using SoundFlow.Providers;
using SoundFlow.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.IO
{
    internal class AudioClipAssetBuilder : AssetBuilderBase
    {
        internal override AssetResourceBase BuildAsset(AssetInfo info, AssetMetaFileBase meta, Guid guid, BinaryReader reader)
        {
            var sampleRate = reader.ReadInt32();
            var channels = reader.ReadInt32();
            var sampleFormat = reader.ReadInt32();
            var samplesLength = reader.ReadInt32();

            var data = new float[samplesLength];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = reader.ReadSingle();
            }

            return new AudioClip(info.Path, 
                                 guid, data, sampleRate, 
                                 samplesLength, channels, sampleFormat);
        }
    }
}