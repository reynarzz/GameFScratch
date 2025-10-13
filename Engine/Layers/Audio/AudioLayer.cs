using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Modifiers;
using SoundFlow.Providers;
using SoundFlow.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class AudioLayer : LayerBase
    {
        private static MiniAudioEngine _engine;
        private static AudioPlaybackDevice _currentDevice;

        private readonly AudioFormat DefaultFormat = new AudioFormat()
        {
            Channels = 2,
            SampleRate = 48000,
            Format = SampleFormat.F32
        };

        public override void Initialize()
        {
            _engine = new MiniAudioEngine();
            var defaultDevice = _engine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
            if (!defaultDevice.IsDefault)
            {
                throw new Exception("No default playback device found.");
            }
            _currentDevice = _engine.InitializePlaybackDevice(defaultDevice, DefaultFormat);
            _currentDevice.Start();

            //var pitchModifier = new AlgorithmicReverbModifier(format);
            //pitchModifier.Wet = 0.7f;
            //pitchModifier.Width = 1.0f;
            //pitchModifier.Mix = 1.0f;
            //pitchModifier.PreDelay = 30;
            //pitchModifier.RoomSize = 1.0f;
            //player.AddModifier(pitchModifier);

            // Effects:
            // AlgorithmicReverbModifier
            // ChorusModifier
            // DelayModifier 
            // ParametricEqualizer 
            // CompressorModifier 
            // 
            // 
            // 
        }

        public static SoundPlayer GetSoundPlayer(AudioFormat format, RawDataProvider provider)
        {
            var player = new SoundPlayer(_engine, format, provider);
            _currentDevice.MasterMixer.AddComponent(player);
            return player;
        }

        public override void Close()
        {
            _engine.Dispose();
        }
    }
}
