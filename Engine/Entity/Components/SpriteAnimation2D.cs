using Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    [UniqueComponent]
    public class SpriteAnimation2D : ScriptBehavior
    {
        private Sprite[] _frames;
        public int FPS { get; set; } = 12;
        private float _accumulator = 0.0f;
        private int _currentFrame;
        public bool Loop { get; set; } = true;
        public bool StartOnAwake { get; set; } = true;
        private bool _isPlaying = false;
        public Renderer2D Renderer { get; set; }

        internal override void OnInitialize()
        {
        }

        public override void OnAwake()
        {
            if (StartOnAwake)
            {
                Play();
            }
        }
        public override void OnUpdate()
        {
            if (_frames == null)
                return;

            var frameDuration = 1.0f / (float)FPS;
            _accumulator += Time.DeltaTime;

            if (_isPlaying && _accumulator >= frameDuration)
            {
                _accumulator -= frameDuration;

                _currentFrame++;
                if (Loop && _currentFrame >= _frames.Length)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = Math.Min(_currentFrame, _frames.Length - 1);
                }

                Renderer.Sprite = _frames[_currentFrame];
            }
        }

        public void PushFrames(Sprite[] sprite)
        {
            if (sprite == _frames)
                return;

            _currentFrame = 0;
            _frames = (sprite);
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
            _isPlaying = false;
            _currentFrame = 0;
        }
    }
}
