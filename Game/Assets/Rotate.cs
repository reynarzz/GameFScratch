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
        private bool _show = false;
        public override void OnUpdate()
        {
            Transform.WorldEulerAngles += new GlmNet.vec3(0, 0, Time.DeltaTime * 50);
        }
    }
}
