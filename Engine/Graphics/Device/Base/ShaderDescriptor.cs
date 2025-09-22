using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class ShaderDescriptor : ResourceDescriptorBase
    {
        public byte[] VertexSource { get; set; }
        public byte[] FragmentSource { get; set; }
    }
}
