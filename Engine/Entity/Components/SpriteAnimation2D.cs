using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class SpriteAnimation2D : Component
    {
        public List<Sprite> _frames;
        public int FPS { get; set; } = 12;
        private float _accumulator = 0.0f;
        private int _currentFrame;
        public bool Loop { get; set; } = true;
        private bool _isPlaying = false;
        public Renderer2D Renderer { get; set; }

        internal override void OnInitialize()
        {
            _frames = new List<Sprite>();
        }

        public void Update()
        {
            var frameDuration = 1.0f / (float)FPS;
            _accumulator = Time.DeltaTime;

            if (_isPlaying && _accumulator >= frameDuration)
            {
                _accumulator -= frameDuration;

                _currentFrame = (_currentFrame + 1) % _frames.Count;
                if (Loop && _currentFrame >= _frames.Count)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = Math.Min(_currentFrame, _frames.Count - 1);
                }

                Renderer.Sprite = _frames[_currentFrame];
            }
        }

        public void PushFrame(Sprite sprite)
        {
            _frames.Add(sprite);
        }

        public void Play()
        {
            if (_isPlaying)
            {
                Stop();
            }

            _isPlaying = true;
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        public void Stop()
        {
            _currentFrame = 0;
        }
    }
}
