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
        private RenderTexture _renderTexture;
        public PostProcessingSinglePass(Shader shader)
        {
            _shader = shader;
            _renderTexture = new RenderTexture(Window.Width, Window.Height);
            Window.OnWindowChanged += UpdateRenderTargetSize;
        }

        public void UpdateRenderTargetSize(int width, int height)
        {
            _renderTexture.UpdateTarget(width, height);
        }

        public override RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture> draw)
        {
            draw(_shader, inRenderTexture, _renderTexture);

            return _renderTexture;
        }

        public override void Dispose()
        {
            Window.OnWindowChanged -= UpdateRenderTargetSize;
            _renderTexture.OnDestroy();
        }
    }
}
