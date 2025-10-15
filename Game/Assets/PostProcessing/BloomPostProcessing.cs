using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class BloomPostProcessing : PostProcessingPass
    {
        private Shader _brightPassShader;
        private Shader _blurPassShader;
        private Shader _blurPassShader2;
        private Shader _combinePassShader;

        private RenderTexture _brightRenderTexture;
        private RenderTexture _combineRenderTexture;
        private RenderTexture _blurRenderTexture;
        private RenderTexture _blurRenderTexture2;
        private PassUniform[] _passUniforms = new PassUniform[1];
        public BloomPostProcessing()
        {
            var vertex = Assets.GetText("Shaders/ScreenVert.vert").Text;

            _brightPassShader = new Shader(vertex, Assets.GetText("Shaders/Bloom/Bloom_BrightPass.frag").Text);
            _blurPassShader = new Shader(vertex, Assets.GetText("Shaders/Bloom/Bloom_GaussianBlur.frag").Text);
            _blurPassShader2 = new Shader(vertex, Assets.GetText("Shaders/Bloom/Bloom_GaussianBlur_vTest.frag").Text);
            _combinePassShader = new Shader(vertex, Assets.GetText("Shaders/Bloom/Bloom_Combine.frag").Text);

            _brightRenderTexture = new RenderTexture(Window.Width / 3, Window.Height / 3);
            _blurRenderTexture = new RenderTexture(_brightRenderTexture.Width, _brightRenderTexture.Height);
            _blurRenderTexture2 = new RenderTexture(_brightRenderTexture.Width, _brightRenderTexture.Height);
            _combineRenderTexture = new RenderTexture(Window.Width, Window.Height);

            _passUniforms = [new PassUniform() { UniformName = "uBlurTex", RenderTexture = _blurRenderTexture2 }];
            Window.OnWindowChanged += Window_OnWindowChanged;
        }

        private void Window_OnWindowChanged(int width, int height)
        {
            _brightRenderTexture.UpdateTarget(width / 3, height / 3);
            _combineRenderTexture.UpdateTarget(width, height);
            _blurRenderTexture.UpdateTarget(_brightRenderTexture.Width, _brightRenderTexture.Height);
            _blurRenderTexture2.UpdateTarget(_brightRenderTexture.Width, _brightRenderTexture.Height);
        }

        public override RenderTexture Render(RenderTexture inRenderTexture, Action<Shader, RenderTexture, RenderTexture, PassUniform[]> draw)
        {
            draw(_brightPassShader, inRenderTexture, _brightRenderTexture, null);
            draw(_blurPassShader, _brightRenderTexture, _blurRenderTexture, null);
            draw(_blurPassShader2, _blurRenderTexture, _blurRenderTexture2, null);
            draw(_combinePassShader, inRenderTexture, _combineRenderTexture, _passUniforms);

            return _combineRenderTexture;
        }

        public override void Dispose()
        {
            _brightPassShader.OnDestroy();
        }
    }
}
