using Engine;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class Respawner : ScriptBehavior
    {
        public vec3 RespawnPosition { get; set; } = new vec3(-7, -4, 0);

        public override void OnStart()
        {
            AddComponent<RigidBody2D>().BodyType = Body2DType.Kinematic;
            var col = AddComponent<BoxCollider2D>();
            col.IsTrigger = true;
            col.Size = new vec2(40, 2);
            Transform.WorldPosition = new vec3(0, -9, 0);
        }

        public override void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.Name.Equals("Player"))
            {
                Debug.Log("Respawn");
                collider.RigidBody.Velocity = default;
                collider.Transform.WorldPosition = RespawnPosition;
            }
        }
    }
}
