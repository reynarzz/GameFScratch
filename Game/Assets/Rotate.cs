using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Rotate : ScriptBehavior
    {
        private float _zOffset;
        private float _rotateSpeed = 0;

        public override void OnStart()
        {
            _zOffset = new Random().NextSingle() * 360;
            _rotateSpeed = RandomRange(30, 50);
        }

        private float RandomRange(float min, float max)
        {
            return min + (float)new Random().NextDouble() * (max - min);
        }

        public override void OnUpdate()
        {
            _zOffset += Time.DeltaTime * _rotateSpeed;

            Transform.WorldEulerAngles = new GlmNet.vec3(0, 0, _zOffset);
        }
    }
}
