using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Types
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequiredComponentAttribute : Attribute
    {
        public Type[] RequiredComponents { get; }
        public RequiredComponentAttribute(params Type[] components)
        {
            RequiredComponents = components;
        }
    }
}
