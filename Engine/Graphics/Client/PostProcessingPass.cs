using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public abstract class PostProcessingPass : IDisposable
    {
        public abstract void Dispose();
        public abstract RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture> draw);

        public void SetProperty(Shader shader, string name, UniformValue value)
        {
            // TODO: set property here


        }
    }
}
