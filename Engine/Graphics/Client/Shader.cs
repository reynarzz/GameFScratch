using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Shader : EObject
    {
        internal GfxResource NativeShader { get; }
        public Shader(string vertexCode, string fragmentCode)
        {
            var shaderDescriptor = new ShaderDescriptor();
            shaderDescriptor.VertexSource = Encoding.UTF8.GetBytes(vertexCode);
            shaderDescriptor.FragmentSource = Encoding.UTF8.GetBytes(fragmentCode);

            NativeShader = GfxDeviceManager.Current.CreateShader(shaderDescriptor);
        }
    }
}
