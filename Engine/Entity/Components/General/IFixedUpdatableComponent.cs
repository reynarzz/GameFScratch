using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    internal interface IFixedUpdatableComponent : IComponent
    {
        void OnFixedUpdate();
    }
}
