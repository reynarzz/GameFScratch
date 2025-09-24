using Engine;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class CameraFollow : ScriptBehavior
    {
        public Transform Target { get; set; }
        public override void OnUpdate()
        {
            if (Target)
            {
                var targetPos = Target.WorldPosition;
                targetPos.z = Transform.WorldPosition.z;

                Transform.WorldPosition = Mathf.Lerp(Transform.WorldPosition, targetPos, Time.DeltaTime);
            }
        }
    }
}
