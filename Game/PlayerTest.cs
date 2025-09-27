using Engine;
using Engine.Layers;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class PlayerTest : ScriptBehavior
    {
        private bool _canJump = false;
        private bool _extraJumpAvailable = false;
        private float _jumpForce = 9;
        public override void OnAwake()
        {
            Debug.Info("Awake");
            //new Actor<RotateTest>();
            var rigid = GetComponent<RigidBody2D>();
            rigid.IsContinuos = true;
            rigid.GravityScale = 2;

            GetComponent<Collider2D>().Friction = 0;
        }

        public override void OnStart()
        {
            Debug.Info("Start");
            //new Actor<RotateTest>();
        }

        public override void OnUpdate()
        {
            if ((_canJump || _extraJumpAvailable) && Input.GetKeyDown(KeyCode.Space))
            {
                _extraJumpAvailable = false;
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(GetComponent<RigidBody2D>().Velocity.x, 0);
                GetComponent<RigidBody2D>()?.AddForce(new GlmNet.vec2(0, _jumpForce), ForceMode2D.Impulse);
            }

            if (Input.GetKey(KeyCode.A))
            {
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(-2, GetComponent<RigidBody2D>().Velocity.y);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(2, GetComponent<RigidBody2D>().Velocity.y);
            }
            else
            {
                GetComponent<RigidBody2D>().Velocity = new GlmNet.vec2(0, GetComponent<RigidBody2D>().Velocity.y);

            }


            var length = 0.4f;
            var origin1 = Transform.WorldPosition + new vec3(-0.45f, -0.55f, 0);
            var origin2 = Transform.WorldPosition + new vec3(0.45f, -0.55f, 0);
            Debug.DrawRay(Transform.WorldPosition, Transform.Up, Color.Green);

            var hitA = Physics2D.Raycast(origin1, Transform.Down * length);
            var hitB = Physics2D.Raycast(origin2, Transform.Down * length);

            var color1 = Color.White;
            var color2 = Color.White;
            if (hitA.isHit || hitB.isHit)
            {
                if (hitA.isHit)
                {
                   // Debug.Log("RayHit: " + hitA.Collider.Name);
                    color1 = Color.Red;
                    Debug.DrawRay(origin1 + Transform.Down * length, new vec3(hitA.Normal.x, hitA.Normal.y, 0), Color.Blue);
                }

                if (hitB.isHit)
                {
                    Debug.DrawRay(origin2 + Transform.Down * length, new vec3(hitB.Normal.x, hitB.Normal.y, 0), Color.Blue);
                    color2 = Color.Red;
                }

                _canJump = true;
                _extraJumpAvailable = true;
            }
            else
            {
                _canJump = false;
            }

            Debug.DrawRay(origin1, Transform.Down * length, color1);
            Debug.DrawRay(origin2, Transform.Down * length, color2);

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
            //List<ContactPoint2D> contacts = null;
            //collision.GetContacts(ref contacts);

            //for (int i = 0; i < contacts.Count; i++)
            //{
            //    Debug.Log(contacts[i].Position);
            //}
            //Debug.Info("Player Collision enter: " + collision.OtherCollider.Name);
            // Actor.Destroy(collision.Actor);
        }

        public override void OnCollisionExit2D(Collision2D collision)
        {
            //Debug.Info("Player Collision -exit: " + collision.OtherCollider.Name);
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
