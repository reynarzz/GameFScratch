using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class EObject
    {
        public string Name { get; set; }

        private const string DefaultObjectName = "Object";

        private Guid _guid;

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
            _guid = Guid.Parse(id);
        }

        public Guid GetID()
        {
            return _guid;
        }
    }
}
