using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public abstract class PostProcessingPass : IDisposable
    {
        public struct PassUniform
        {
            public string UniformName { get; set; }
            public RenderTexture RenderTexture { get; set; }
        }

        public abstract RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture, PassUniform[]> draw);

        public void SetProperty(Shader shader, string name, UniformValue value)
        {
            // TODO: set property here


        }

        public abstract void Dispose();
    }
}
