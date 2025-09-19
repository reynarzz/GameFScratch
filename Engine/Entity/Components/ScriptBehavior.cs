using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    // All behaviors need to inherit from this class
    public abstract class ScriptBehavior : Component
    {
        public virtual void OnAwake() { }
        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnTriggerEnter2D() { }
        public virtual void OnTriggerExit2D() { }
        public virtual void OnDestroy() { }
    }
}