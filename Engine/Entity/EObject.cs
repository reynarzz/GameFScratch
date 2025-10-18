using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Exceptions;
using Engine.Layers;

namespace Engine
{
    public abstract class EObject : IObject
    {
        public virtual string Name { get; set; } = DefaultObjectName;

        private const string DefaultObjectName = "Object";

        private Guid _guid;

        internal bool IsAlive { get; set; } = true;
        internal bool IsPendingToDestroy { get; set; } = false;

        public EObject()
        {
            _guid = Guid.NewGuid();
        }

        public EObject(string name)
        {
            Name = name;
            _guid = Guid.NewGuid();
        }

        public EObject(string name, string id)
        {
            Name = name;

            if (!string.IsNullOrEmpty(id))
            {
                _guid = Guid.Parse(id);
            }
            else
            {
                _guid = Guid.NewGuid();
            }
        }

        public EObject(string name, Guid id)
        {
            Name = name;
            _guid = id;
        }

        public Guid GetID()
        {
            return _guid;
        }

        public static implicit operator bool(EObject obj)
        {
            return obj != null && obj.IsAlive && !obj.IsPendingToDestroy && obj._guid != Guid.Empty;
        }

        protected void CheckIfValidObject(EObject obj)
        {
            if (CleanUpLayer.CleaningUp)
                return;

            if (!obj.IsAlive)
            {
                throw new DestroyedObjectException($"Can't use destroyed object of type: '{obj.GetType().Name}'");
            }
            else if (obj.IsPendingToDestroy)
            {
                Debug.Error($"Object of type: '{obj.GetType().Name}' was marked to be destroyed, please don't use it.");
            }
        }

#if DEBUG
        ~EObject()
        {
            Debug.Info($"Mem Destroy: ({GetType().Name}), name: {Name}");
        }
#endif

        public virtual void OnDestroy() { }

        public bool IsValid()
        {
            return this;
        }
    }
}