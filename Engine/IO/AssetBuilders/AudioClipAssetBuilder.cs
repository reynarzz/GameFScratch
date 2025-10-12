using SharedTypes;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Interfaces;
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

            
            // Create SoundPlayer and attach to mixer
            using var provider = new FloatArrayProvider(RawData);
            var player = new SoundPlayer(engine, format, provider);
            device.MasterMixer.AddComponent(player);

            // Start device and play
            device.Start();
            player.Play();

            //// Stop and cleanup
            //player.Stop();
            //device.Stop();
        }


        internal class FloatArrayProvider : ISoundDataProvider
        {
            private readonly float[] _samples;
            private int _position;

            internal FloatArrayProvider(float[] samples)
            {
                _samples = samples;
                _position = 0;
            }

            public int ReadBytes(Span<float> buffer)
            {
                int count = Math.Min(buffer.Length, _samples.Length - _position);
                _samples.AsSpan(_position, count).CopyTo(buffer);
                _position += count;
                return count;
            }

            public void Seek(int offset) => _position = Math.Clamp(offset, 0, _samples.Length);
            public int Position => _position;
            public int Length => _samples.Length;
            public bool CanSeek => true;
            public SampleFormat SampleFormat => SampleFormat.F32;
            public int SampleRate => 48000;
            public bool IsDisposed => false;
            public event EventHandler<EventArgs> EndOfStreamReached;
            public event EventHandler<PositionChangedEventArgs> PositionChanged;
            public void Dispose() { }
        }

    }
}
