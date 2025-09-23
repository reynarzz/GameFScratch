using System;
using System.Diagnostics;

namespace Engine.Layers
{
  

    internal class TimeLayer : LayerBase
    {
        private Stopwatch _stopwatch;
        private float _lastFrameTime;
        private float _timePast;
        public override void Initialize()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _lastFrameTime = 0f;
            _timePast = 0;
        }

        internal override void UpdateLayer()
        {
            float currentTime = (float)_stopwatch.Elapsed.TotalSeconds;
            float deltaTime = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;
            _timePast += deltaTime;

            // Update the Time static class
            Time.DeltaTime = deltaTime * Time.TimeScale;
            Time.SinceStarted = currentTime * Time.TimeScale;
            Time.FPS = Time.DeltaTime > 0f ? 1f / Time.DeltaTime : 0f;
            Time.TimeCurrent = _timePast;
        }

        public override void Close()
        {
            _stopwatch.Stop();
        }
    }
}