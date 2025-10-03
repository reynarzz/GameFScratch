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
        public float FollowSpeed { get; set; } = 10f;
        public override void OnUpdate()
        {
            if (Target)
            {
                var targetPos = Target.WorldPosition;
                targetPos.z = Transform.WorldPosition.z;

                var smoothPos = Mathf.SineLerp(Transform.WorldPosition, targetPos, FollowSpeed * Time.DeltaTime);
                vec2 pixelSize = new vec2(1f / 16.0f);
                vec2 snappedPos = new vec2(
                    MathF.Round(smoothPos.x / pixelSize.x) * pixelSize.x,
                    MathF.Round(smoothPos.y / pixelSize.y) * pixelSize.y
                );

                Transform.WorldPosition = smoothPos; // new vec3(snappedPos, Transform.WorldPosition.z);
            }
        }
    }
}
