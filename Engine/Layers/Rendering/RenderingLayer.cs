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
        private DrawCallData _drawCallData;
        private PipelineFeatures _pipelineFeatures;

        public RenderingLayer()
        {

        }

        public override void Initialize()
        {
            _batcher2d = new Batcher2D(Consts.Graphics.MAX_QUADS_PER_BATCH);
            _pipelineFeatures = new PipelineFeatures();

            _drawCallData = new DrawCallData()
            {
                Textures = new GfxResource[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits],
                Uniforms = new UniformValue[Consts.Graphics.MAX_UNIFORMS_PER_DRAWCALL],
            };
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
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = _mainCamera.BackgroundColor, RenderTarget = _mainCamera.RenderTexture?.NativeResource });

            // TODO: improve this, don't ask for renderers but add/remove with events.
            var batches = _batcher2d.GetBatches(SceneManager.ActiveScene.FindAll<Renderer2D>(findDisabled: false));

            var VP = _mainCamera.Projection * _mainCamera.ViewMatrix;

            foreach (var batch in batches)
            {
                if (!batch.IsActive)
                    break;

                batch.Flush();

                for (int i = 0; i < batch.Textures.Length; i++)
                {
                    var tex = batch.Textures[i];
                    if (tex == null)
                        break;
                    _drawCallData.Textures[i] = tex.NativeResource;
                }

                // Pipeline
                _pipelineFeatures.Blending = batch.Material.Blending;
                _pipelineFeatures.Stencil = batch.Material.Stencil;

                _drawCallData.DrawType = batch.DrawType;
                _drawCallData.DrawMode = batch.DrawMode;
                _drawCallData.IndexedDraw.IndexCount = batch.IndexCount;
                _drawCallData.Shader = batch.Material.Shader.NativeShader;
                _drawCallData.Geometry = batch.Geometry;
                _drawCallData.Features = _pipelineFeatures;
                _drawCallData.RenderTarget = _mainCamera.RenderTexture?.NativeResource;

                // Iniforms
                _drawCallData.Uniforms[Consts.Graphics.VP_MATRIX_UNIFORM_INDEX].SetMat4(Consts.VIEW_PROJ_UNIFORM_NAME, VP);
                _drawCallData.Uniforms[Consts.Graphics.TEXTURES_ARRAY_UNIFORM_INDEX].SetIntArr(Consts.TEX_ARRAY_UNIFORM_NAME, Batch2D.TextureSlotArray);
                _drawCallData.Uniforms[Consts.Graphics.MODEL_MATRIX_UNIFORM_INDEX].SetMat4(Consts.MODEL_UNIFORM_NAME, batch.WorldMatrix);

                // Draw
                GfxDeviceManager.Current.Draw(_drawCallData);
            }


            Debug.DrawGeometries(VP, _drawCallData.RenderTarget);

            GfxDeviceManager.Current.Present(_drawCallData.RenderTarget);

        }
    }
}