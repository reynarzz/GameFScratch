using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Audio
{
    // Helper wrapper for raw PCM AudioClip
    internal class RawAudioStreamWrapper : WaveStream
    {
        private readonly RawSourceWaveStream _rawStream;

        internal RawAudioStreamWrapper(AudioClip clip)
        {
            var format = new WaveFormat(clip.SampleRate, clip.BitsPerSample, clip.Channels);
            _rawStream = new RawSourceWaveStream(clip.RawPCM, 0, clip.RawPCM.Length, format);
        }

        public override WaveFormat WaveFormat => _rawStream.WaveFormat;
        public override long Length => _rawStream.Length;
        public override long Position { get => _rawStream.Position; set => _rawStream.Position = value; }
        public override int Read(byte[] buffer, int offset, int count) => _rawStream.Read(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing) _rawStream.Dispose();
            base.Dispose(disposing);
        }
    }
}
