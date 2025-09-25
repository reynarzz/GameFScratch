using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class RotateTest : ScriptBehavior
    {
        float angle = 0;

        public override void OnStart()
        {
            Debug.Info("Start Rotate: " + Name);    
        }

        public override void OnUpdate()
        {
            angle += Time.DeltaTime * 50;
            Transform.LocalEulerAngles = new GlmNet.vec3(0, 0, angle);
        }

        public override void OnFixedUpdate()
        {
        }
    }
}
