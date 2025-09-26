using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class EObject
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

        public Guid GetID()
        {
            return _guid;
        }

        public static implicit operator bool(EObject obj)
        {
            return obj != null && obj.IsAlive && !obj.IsPendingToDestroy && obj._guid != Guid.Empty;
        }

        protected bool IsValidObject(EObject obj)
        {
            if (!obj.IsAlive)
            {
#if DEBUG
                try
                {
                    Debug.Error($"Can't use already deleted object: {obj.GetType().Name}");
                }
                catch (Exception)
                {
                    Debug.Error($"Can't use already deleted object");
                }
#endif
                return false;
            }

            return true;
        }
#if DEBUG
        ~EObject()
        {
            Debug.Info($"Destroyed object: ({GetType().Name}), name: {Name}");
        }
#endif
    }
}