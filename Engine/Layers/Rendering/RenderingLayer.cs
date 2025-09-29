using Engine.Graphics;
using Engine.Graphics.OpenGL;
using Engine.Rendering;
using Engine.Utils;
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
        private Batcher2D _batcher2d;
        private Camera _mainCamera = null;
        private const int MAX_QUADS_PER_BATCH = 5000;

        public RenderingLayer()
        {
        }

        public override void Initialize()
        {
            _batcher2d = new Batcher2D(MAX_QUADS_PER_BATCH);
        }

        public override void Close()
        {
        }

        internal override void UpdateLayer()
        {
            if (!_mainCamera)
            {
                _mainCamera = SceneManager.ActiveScene.FindComponent<Camera>(findDisabled: false);
            }

            if (!_mainCamera || !_mainCamera.IsEnabled)
            {
                Debug.Error("No cameras found in scene.");
                GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = new Color(1, 0, 1, 1) });
                return;
            }

            GfxDeviceManager.Current.SetViewport(_mainCamera.Viewport);
            // Clear screen
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = _mainCamera.BackgroundColor });

            var batches = _batcher2d.CreateBatches(SceneManager.ActiveScene.FindAll<Renderer2D>(findDisabled: false));

            var VP = _mainCamera.Projection * _mainCamera.ViewMatrix;

            foreach (var batch in batches)
            {
                if (!batch.IsActive)
                    break;

                GfxDeviceManager.Current.SetPipelineFeatures(new PipelineFeatures() { Blending = new Blending() { Enabled = true } });

                batch.Flush();
                (batch.Geometry as GLGeometry).Bind();
                var shader = batch.Material.Shader.NativeShader as GLShader;
                shader.Bind();
                shader.SetUniform(Consts.VIEW_PROJECTION_UNIFORM_NAME, VP);

                for (int i = 0; i < batch.Textures.Length; i++)
                {
                    var tex = batch.Textures[i];
                    if (tex == null)
                        break;
                    (tex.NativeTexture as GLTexture).Bind(i);
                }

                shader.SetUniform(Consts.TEXTURES_ARRAY_UNIFORM_NAME, Batch2D.TextureSlotArray);

                // Draw
                GfxDeviceManager.Current.DrawIndexed(DrawMode.Triangles, batch.IndexCount);
            }
            
            Debug.DrawGeometries(VP);
        }
    }
}