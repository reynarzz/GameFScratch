using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public abstract class PostProcessingPass
    {
        public abstract RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture> draw);
    }
}
