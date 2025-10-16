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
            public string Name { get; set; }
            public RenderTexture RenderTexture { get; set; }
        }

        private Action<Shader, RenderTexture, RenderTexture, PassUniform[]> _drawCallback;

        public RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture, PassUniform[]> draw)
        {
            _drawCallback = draw;
            return Render(inRenderTexture);
        }

        protected abstract RenderTexture Render(RenderTexture inRenderTexture);

        protected void Draw(Shader shader, RenderTexture readFrom, RenderTexture applyTo, params PassUniform[] uniforms)
        {
            _drawCallback(shader, readFrom, applyTo, uniforms);
        }

        public virtual void Dispose()
        {
            _drawCallback = null;
        }
    }
}
