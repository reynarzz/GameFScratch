using Engine.Graphics;
using Engine.Rendering;
using Engine.Utils;
using GlmNet;

namespace Engine.Layers
{
    internal class RenderingLayer : LayerBase
    {
        private Batcher2D _batcher2d;
        private Camera _mainCamera = null;
        private DrawCallData _drawCallData;
        private PipelineFeatures _pipelineFeatures;
        private GfxResource _screenGrabTarget;

        public override void Initialize()
        {
            _batcher2d = new Batcher2D(Consts.Graphics.MAX_QUADS_PER_BATCH);
            _pipelineFeatures = new PipelineFeatures();

            _drawCallData = new DrawCallData()
            {
                Textures = new GfxResource[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits],
                Uniforms = new UniformValue[Consts.Graphics.MAX_UNIFORMS_PER_DRAWCALL],
            };

            _screenGrabTarget = GfxDeviceManager.Current.CreateRenderTarget(new RenderTargetDescriptor()
            {
                Width = Window.Width,
                Height = Window.Height,
            });

            Window.OnWindowChanged += OnWindowChanged;
        }

        private void OnWindowChanged(int width, int height)
        {
            GfxDeviceManager.Current.UpdateResouce(_screenGrabTarget, new RenderTargetDescriptor()
            {
                Width = width,
                Height = height,
            });
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
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig()
            {
                Color = _mainCamera.BackgroundColor,
                RenderTarget = _mainCamera.RenderTexture?.NativeResource
            });

            // TODO: improve this, don't ask for renderers but add/remove with events.
            var batches = _batcher2d.GetBatches(SceneManager.ActiveScene.FindAll<Renderer2D>(findDisabled: false));

            var VP = _mainCamera.Projection * _mainCamera.ViewMatrix;

            foreach (var batch in batches)
            {
                if (!batch.IsActive)
                    break;

                batch.Flush();

                var isScreenGrabPass = batch.Material.Passes.Any(x => x.IsScreenGrabPass);

                if (isScreenGrabPass)
                {
                    GfxDeviceManager.Current.Clear(new ClearDeviceConfig()
                    {
                        Color = _mainCamera.BackgroundColor,
                        RenderTarget = _screenGrabTarget
                    });

                    foreach (var batchGrab in batches)
                    {
                        if (!batchGrab.IsActive || batchGrab == batch)
                            break;

                        batchGrab.Flush();
                        RenderPass(batchGrab, ref VP, _screenGrabTarget, null);
                    }
                }

                RenderPass(batch, ref VP, _mainCamera.RenderTexture?.NativeResource, _screenGrabTarget);
            }

            Debug.DrawGeometries(VP, _drawCallData.RenderTarget);

            GfxDeviceManager.Current.Present(_drawCallData.RenderTarget);
        }

        private void RenderPass(Batch2D batch, ref mat4 VP, GfxResource renderTarget, GfxResource screenGrabTarget)
        {
            foreach (var pass in batch.Material.Passes)
            {
                int boundTex = 0;
                for (; boundTex < batch.Textures.Length; boundTex++)
                {
                    var tex = batch.Textures[boundTex];

                    if (tex == null)
                        break;
                    _drawCallData.Textures[boundTex] = tex.NativeResource;
                }

                int screenGrabIndex = boundTex;

                // Grab the color texture
                _drawCallData.Textures[screenGrabIndex] = pass.IsScreenGrabPass ? screenGrabTarget.SubResources[0] : null;

                // Pipeline
                _pipelineFeatures.Blending = pass.Blending;
                _pipelineFeatures.Stencil = pass.Stencil;

                _drawCallData.DrawType = batch.DrawType;
                _drawCallData.DrawMode = batch.DrawMode;
                _drawCallData.IndexedDraw.IndexCount = batch.IndexCount;
                _drawCallData.Shader = pass.Shader.NativeShader;
                _drawCallData.Geometry = batch.Geometry;
                _drawCallData.Features = _pipelineFeatures;
                _drawCallData.RenderTarget = renderTarget;

                // Iniforms
                _drawCallData.Uniforms[Consts.Graphics.VP_MATRIX_UNIFORM_INDEX].SetMat4(Consts.VIEW_PROJ_UNIFORM_NAME, VP);
                _drawCallData.Uniforms[Consts.Graphics.TEXTURES_ARRAY_UNIFORM_INDEX].SetIntArr(Consts.TEX_ARRAY_UNIFORM_NAME, Batch2D.TextureSlotArray);
                _drawCallData.Uniforms[Consts.Graphics.MODEL_MATRIX_UNIFORM_INDEX].SetMat4(Consts.MODEL_UNIFORM_NAME, batch.WorldMatrix);
                _drawCallData.Uniforms[Consts.Graphics.SCREEN_FRAME_BUFFER_GRAB_INDEX].SetInt(Consts.SCREEN_GRAB_TEX_UNIFORM_NAME, screenGrabIndex);
                _drawCallData.Uniforms[Consts.Graphics.SCREEN_SIZE_INDEX].SetVec2(Consts.SCREEN_SIZE_UNIFORM_NAME, new vec2(Window.Width, Window.Height));

                // Draw
                GfxDeviceManager.Current.Draw(_drawCallData);
            }
        }
    }
}