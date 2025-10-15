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
        private readonly RenderTexture _renderTextureOut;
        public PostProcessingSinglePass(Shader shader)
        {
            _shader = shader;
            _renderTextureOut = new RenderTexture(Window.Width, Window.Height);
            Window.OnWindowChanged += UpdateRenderTargetSize;
        }

        public void UpdateRenderTargetSize(int width, int height)
        {
            _renderTextureOut.UpdateTarget(width, height);
        }

        public override RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture, PassUniform[]> draw)
        {
            draw(_shader, inRenderTexture, _renderTextureOut, null);
            return _renderTextureOut;
        }

        public override void Dispose()
        {
            Window.OnWindowChanged -= UpdateRenderTargetSize;
            _renderTextureOut.OnDestroy();
        }
    }
}
