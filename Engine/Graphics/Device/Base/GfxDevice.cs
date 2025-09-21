using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal abstract class GfxDevice
    {
        //internal void Draw();
        internal abstract void CreateGeometry(GeometryDescriptor desc);
        internal abstract void UpdateGeometry(GeometryDescriptor desc);
    }
}
