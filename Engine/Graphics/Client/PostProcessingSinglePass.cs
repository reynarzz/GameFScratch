using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    public class PostProcessingSinglePass : PostProcessingPass
    {
        private readonly Shader _shader;
        public PostProcessingSinglePass(Shader shader)
        {
            _shader = shader;
        }

        public override RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture> draw)
        {
            draw(_shader, inRenderTexture, inRenderTexture);

            return inRenderTexture;
        }
    }
}
