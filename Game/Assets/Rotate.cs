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
        public override void OnStart()
        {
            _zOffset = new Random().NextSingle() * 360;
        }
        public override void OnUpdate()
        {
            _zOffset += Time.DeltaTime * 50;

            Transform.WorldEulerAngles = new GlmNet.vec3(0, 0, _zOffset);
        }
    }
}
