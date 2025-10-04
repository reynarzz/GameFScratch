using Engine;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class Platform : ScriptBehavior
    {
        public vec3[] Points = [ new vec3(-22.5f, -9, 0), new vec3(-22.5f, 6, 0), new vec3(-22.5f, 15, 0)];
        public float Speed { get; set; } = 4f;
        public float WaitTime { get; set; } = 1.5f;
        private float _currentWait = 0;

        private SpriteRenderer _renderer;
        private int _pointIndex = 1;
        private vec3 _startPos;
        public override void OnStart()
        {
            base.OnStart();
            _renderer = AddComponent<SpriteRenderer>();
            _renderer.Color = Color.Black;
            var rigid = AddComponent<RigidBody2D>();
            rigid.BodyType = Body2DType.Kinematic;
            rigid.Interpolate = true;
            var trigger = AddComponent<BoxCollider2D>();
            trigger.Size = new vec2(3, 1);
            trigger.Offset = new vec2(0, 0.03f);
            trigger.IsTrigger = true;
            Transform.LocalScale = new vec3(trigger.Size.x, trigger.Size.y, 1);
            _startPos = Transform.LocalPosition = new vec3(-8, 0, 0);

            AddComponent<BoxCollider2D>().Size = trigger.Size;

            Transform.WorldPosition = _startPos + Points[_pointIndex];
            _currentWait = WaitTime;
            Debug.Log("Platform start");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            var target = _startPos + Points[_pointIndex];

            Transform.WorldPosition = Mathf.MoveTowards(Transform.WorldPosition, target, Time.DeltaTime * Speed);

            var distance = Mathf.Distance(Transform.WorldPosition, target);
            if (distance < 0.001f && (_currentWait -= Time.DeltaTime) <= 0)
            {
                _currentWait = WaitTime;
                if (_pointIndex + 1 >= Points.Length)
                {
                    _pointIndex = 0;
                }
                else
                {
                    _pointIndex++;
                }
            }
        }

        public override void OnTriggerEnter2D(Collider2D collider)
        {
            if(collider.Actor.Layer == LayerMask.NameToLayer("Player"))
            {
                collider.Actor.Transform.Parent = Transform;
                Debug.Log("Enter player to platform");
            }
        }

        public override void OnTriggerExit2D(Collider2D collider)
        {
            collider.Actor.Transform.Parent = null;
        }
    }
}
