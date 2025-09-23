using Engine.Graphics;
using Engine.Graphics.OpenGL;
using Engine.Rendering;
using GlmNet;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Engine.Layers
{
    internal class RenderingLayer : LayerBase
    {
        private List<Renderer> _renderers;

        private Batcher2D _batcher2d;
        private Camera _mainCamera = null;

        public RenderingLayer()
        {
            _batcher2d = new Batcher2D(777);
        }

        public override void Initialize()
        {
        }

        public override void Close()
        {
        }

        internal override void UpdateLayer()
        {
            if (!_mainCamera)
            {
                _mainCamera = SceneManager.ActiveScene.Find<Camera>();
            }

            if (!_mainCamera || !_mainCamera.IsEnabled)
            {
                Log.Error("No cameras found in scene.");
                GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = new vec4(1, 0, 1, 1) });
                return;
            }

            // Clear screen
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = _mainCamera.BackgroundColor });

            var batches = _batcher2d.CreateBatches(SceneManager.ActiveScene.FindAll<Renderer2D>());

            var VP = _mainCamera.Projection * _mainCamera.ViewMatrix;

            foreach (var batch in batches)
            {
                batch.Flush();
                (batch.Geometry as GLGeometry).Bind();
                var shader = batch.Material.Shader.NativeShader as GLShader;
                shader.Bind();

                shader.SetUniform("uVP", VP);

                for (int i = 0; i < batch.Textures.Length; i++)
                {
                    var tex = batch.Textures[i];

                    if (tex == null)
                        break;
                    (tex.NativeTexture as GLTexture).Bind(i);
                }

                shader.SetUniform("uTextures", Batch2D.TextureSlotArray);

                // Draw
                GfxDeviceManager.Current.DrawIndexed(DrawMode.Triangles, batch.IndexCount);
            }
        }
    }
}