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
        private static bool _played = false;
        internal override EObject BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            var sampleRate = reader.ReadInt32();
            var channels = reader.ReadInt32();
            var sampleFormat = reader.ReadInt32();
            var framesRead = reader.ReadInt32();

            var data = new float[framesRead];

            // Read all float samples
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = reader.ReadSingle(); // each sample is a 32-bit float
            }

            Test(data, sampleRate, channels, sampleFormat);
            return new AudioClip(Path.GetFileNameWithoutExtension(info.Path), guid, data, sampleRate, framesRead, channels);
        }
        MiniAudioEngine engine = new MiniAudioEngine();

        private void Test(float[] RawData, int sampleRate, int channels, int sampleFormat)
        {
            if (!_played)
            {
                _played = true;
            }
            else
            {
                return;
            }

            // Initialize default playback device
            var defaultDevice = engine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
            if (!defaultDevice.IsDefault)
            {
                throw new Exception("No default playback device found.");
            }

            // Create a custom AudioFormat matching your data
            var format = new AudioFormat()
            {
                Channels = channels,
                SampleRate = sampleRate,
                Format = (SampleFormat)sampleFormat
            };

            var device = engine.InitializePlaybackDevice(defaultDevice, format);


            var provider = new RawDataProvider(RawData);
            var player = new SoundPlayer(engine, format, provider);

            var mixer = new Mixer(engine, format);
            mixer.AddComponent(player);
            
            device.MasterMixer.AddComponent(mixer);


            // Start device and play
            device.Start();
            //player.PlaybackSpeed = 1;

            player.Play();
            player.Seek(4);

            player.IsLooping = true;
            //player.PlaybackSpeed = 2;
            //player.Pan = 0.7f;
            var pitchModifier = new AlgorithmicReverbModifier(format);
            pitchModifier.Wet = 0.7f;
            pitchModifier.Width = 1.0f;
            pitchModifier.Mix = 1.0f;
            pitchModifier.PreDelay = 30;
            pitchModifier.RoomSize = 1.0f;
            player.AddModifier(pitchModifier);

            // Effects:
            // AlgorithmicReverbModifier
            // ChorusModifier
            // DelayModifier 
            // ParametricEqualizer 
            // CompressorModifier 
            // 
            // 
            // 




            //player.Volume = 0.5f;
            //// Stop and cleanup
            //player.Stop();
            //device.Stop();
        }
    }
}
