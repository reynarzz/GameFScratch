using Engine;
using Engine.Layers;
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
            Debug.Info("Awake");
            //new Actor<RotateTest>();
            GetComponent<RigidBody2D>().IsContinuos = true;
        }

        public override void OnStart()
        {
            Debug.Info("Start");
            //new Actor<RotateTest>();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(GetComponent<RigidBody2D>().Velocity.x, 0);
                GetComponent<RigidBody2D>()?.AddForce(new GlmNet.vec2(0, 6), ForceMode2D.Impulse);
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

        public override void OnCollisionEnter2D(Collision2D collision)
        {
            List<ContactPoint2D> contacts = null;
            collision.GetContacts(ref contacts);

            for (int i = 0; i < contacts.Count; i++)
            {
                Debug.Log(contacts[i].Position);
            }
            Debug.Info("Player Collision enter: " + collision.OtherCollider.Name);
             Actor.Destroy(collision.Actor);
        }
         
        public override void OnCollisionExit2D(Collision2D collision)
        {
            Debug.Info("Player Collision -exit: " + collision.OtherCollider.Name);
        }

        public override void OnCollisionStay2D(Collision2D collision)
        {
            // Debug.Info("Player Collision stay: " + collision.OtherCollider.Name);
        }
        public override void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log("On trigger enter: " + collider.Name);
            //Actor.Destroy(collider.Actor);
        }
    }
}
