using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class EObject
    {
        public string Name { get; set; }

        private const string DefaultObjectName = "Object";

        private Guid _guid;

        internal bool IsAlive { get; set; } = true;
        internal bool IsPendingToDestroy { get; set; } = false;

        public EObject()
        {
            Name = DefaultObjectName;
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
            if (!obj)
            {
                Log.Error("Can't use already deleted object");
                return false;
            }

            return true;
        }

        ~EObject()
        {
            Log.Info($"Destroyed object: ({GetType().Name}), name: {Name}");
        }
    }
}