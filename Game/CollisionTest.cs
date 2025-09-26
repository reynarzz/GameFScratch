using Engine;
using Engine.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class CollisionTest : ScriptBehavior
    {
        public override void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Info($"{collision.Actor.Name}: Collision enter: " + collision.OtherCollider.Name);
        }

        public override void OnCollisionExit2D(Collision2D collision)
        {
            Debug.Info($"{collision.Actor.Name}: Collision -exit: " + collision.OtherCollider.Name);
        }
        public override void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Info($"{collider.Actor.Name}: Trigger Enter: " + collider.Name);

        }
        public override void OnTriggerExit2D(Collider2D collider)
        {
            Debug.Info($"{collider.Actor.Name}: Trigger ~exit: " + collider.Name);

        }
    }
}
