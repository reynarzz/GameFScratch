using Engine.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Base class for all scripts
    /// </summary>
    public abstract class ScriptBehavior : Component
    {
        public virtual void OnAwake() { }
        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnCollisionEnter2D(Collision2D collision) { }
        public virtual void OnCollisionExit2D(Collision2D collision) { }
        public virtual void OnCollisionStay2D(Collision2D collision) { }
        public virtual void OnTriggerEnter2D(Collider2D collider) { }
        public virtual void OnTriggerStay2D(Collider2D collider) { }
        public virtual void OnTriggerExit2D(Collider2D collider) { }
    }
}