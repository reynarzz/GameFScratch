using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class PlayerTest : ScriptBehavior
    {
        public override void OnAwake()
        {
            Log.Info("Awake");
            //new Actor<RotateTest>();

        }
        public override void OnStart()
        {
            Log.Info("Start");
            //new Actor<RotateTest>();
        }

        public override void OnUpdate()
        {
            GetComponent<RigidBody2D>().IsContinuos = true;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(GetComponent<RigidBody2D>().Velocity.x, 0);
                GetComponent<RigidBody2D>()?.AddForce(new GlmNet.vec2(0, 4), ForceMode2D.Impulse);
            }

            if (Input.GetKey(KeyCode.A))
            {
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(-1, GetComponent<RigidBody2D>().Velocity.y);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(1, GetComponent<RigidBody2D>().Velocity.y);
            }
            else
            {
            }
        }

        public override void OnFixedUpdate()
        {
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                //GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(0, GetComponent<RigidBody2D>().Velocity.y);

            }
        }
    }
}
