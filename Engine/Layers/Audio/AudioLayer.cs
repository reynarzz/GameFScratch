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
        private static AudioMixer _masterMixer;

        private static readonly AudioFormat DefaultFormat = AudioFormat.DvdHq;

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

            _masterMixer = new AudioMixer("Master", _currentDevice.MasterMixer);

            // Effects:
            // ParametricEqualizer 
        }

        internal static SoundPlayer CreateSoundPlayer(AudioFormat format, AudioMixer mixer, RawDataProvider provider)
        {
            var player = new SoundPlayer(_engine, format, provider);

            if(mixer != null)
            {
                mixer.AddPlayer(player);
            }
            else
            {
                _currentDevice.MasterMixer.AddComponent(player);
            }

            return player;
        }

        public override void Close()
        {
            _engine.Dispose();
        }

        internal static AudioPlaybackDevice GetDevice()
        {
            return _currentDevice;
        }

        internal static Mixer CreateMixer()
        {
            var mixer = new Mixer(_engine, DefaultFormat);
            _currentDevice.MasterMixer.AddComponent(mixer);
            return mixer;
        }

        internal static AudioMixer GetMasterAudioMixer()
        {
            return _masterMixer;
        }
    }
}
