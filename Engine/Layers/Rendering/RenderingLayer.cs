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
        private DrawCallData _screenQuadDrawCallData;
        private PipelineFeatures _pipelineFeatures;
        private PipelineFeatures _screenPipelineFeatures;
        private GfxResource _screenGrabTarget;
        private GfxResource _screenGeometry;
        private RenderTexture _defaultSceneRenderTexture;
        private Shader _screenShader;
        private PostProcessingStack _postProcessStack;

        public override void Initialize()
        {
            _batcher2d = new Batcher2D(Consts.Graphics.MAX_QUADS_PER_BATCH);
            _pipelineFeatures = new PipelineFeatures();
            _screenPipelineFeatures = new PipelineFeatures();

            _drawCallData = new DrawCallData()
            {
                Textures = new GfxResource[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits],
                Uniforms = new UniformValue[Consts.Graphics.MAX_UNIFORMS_PER_DRAWCALL],
            };

            _screenQuadDrawCallData = new DrawCallData()
            {
                Textures = new GfxResource[1],
                Uniforms = new UniformValue[Consts.Graphics.MAX_UNIFORMS_PER_DRAWCALL],
            };

            _screenGrabTarget = GfxDeviceManager.Current.CreateRenderTarget(new RenderTargetDescriptor()
            {
                Width = Window.Width,
                Height = Window.Height,
            });

            _defaultSceneRenderTexture = new RenderTexture(GfxDeviceManager.Current.CreateRenderTarget(new RenderTargetDescriptor()
            {
                Width = Window.Width,
                Height = Window.Height,
            }), Window.Width, Window.Height);

            _screenGeometry = GraphicsHelper.GetScreenQuadGeometry();
            _screenShader = new Shader(Tests.QuadVertexShader, Tests.QuadFragmentShader);

            Window.OnWindowChanged += OnUpdateScreenGrabPass;
        }

        private void OnUpdateScreenGrabPass(int width, int height)
        {
            var w = _mainCamera.RenderTexture?.Width ?? width;
            var h = _mainCamera.RenderTexture?.Height ?? height;

            GfxDeviceManager.Current.UpdateResouce(_screenGrabTarget, new RenderTargetDescriptor()
            {
                Width = w,
                Height = h
            });

            GfxDeviceManager.Current.UpdateResouce(_defaultSceneRenderTexture.NativeResource, new RenderTargetDescriptor()
            {
                Width = w,
                Height = h,
            });
        }

        internal override void UpdateLayer()
        {
            if (!_mainCamera)
            {
                _mainCamera = SceneManager.ActiveScene.FindComponent<Camera>(findDisabled: false);

                if (_mainCamera != null && _mainCamera.RenderTexture)
                {
                    OnUpdateScreenGrabPass(_mainCamera.RenderTexture.Width, _mainCamera.RenderTexture.Height);
                }
            }

            if (!_mainCamera || !_mainCamera.IsEnabled)
            {
                Debug.Error("No cameras found in scene.");
                GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = new Color(1, 0, 1, 1) });
                return;
            }

            var sceneRenderTarget = _mainCamera.RenderTexture ?? _defaultSceneRenderTexture;
            GfxDeviceManager.Current.SetViewport(_mainCamera.Viewport);

            // Clear screen
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig()
            {
                Color = _mainCamera.BackgroundColor,
                RenderTarget = sceneRenderTarget.NativeResource
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
                        RenderPass(batchGrab, ref VP, _screenGrabTarget, _screenGrabTarget);
                    }
                }

                RenderPass(batch, ref VP, sceneRenderTarget.NativeResource, _screenGrabTarget);
            }


            foreach (var pass in PostProcessingStack.Passes)
            {
                void PostProcessRenderPass(Shader shader, RenderTexture inTex, RenderTexture outTex)
                {
                    DrawScreenQuad(shader, VP, inTex.NativeResource, outTex.NativeResource);
                }

                sceneRenderTarget = pass.Render(sceneRenderTarget, PostProcessRenderPass);
            }

            Debug.DrawGeometries(VP, sceneRenderTarget.NativeResource);

            DrawScreenQuad(_screenShader, VP, sceneRenderTarget.NativeResource, null);

            GfxDeviceManager.Current.Present();

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

                // Set material's texture
                foreach (var texture in batch.Material.Textures)
                {
                    _drawCallData.Textures[++boundTex] = texture.NativeResource;
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
                _drawCallData.Uniforms[Consts.Graphics.APP_TIME_INDEX].SetVec3(Consts.TIME_UNIFORM_NAME, new vec3(Time.TimeCurrent, Time.TimeCurrent * 2, Time.TimeCurrent * 3));

                // Draw
                GfxDeviceManager.Current.Draw(_drawCallData);
            }
        }

        private void DrawScreenQuad(Shader shader, mat4 VP, GfxResource sceneRenderTarget, GfxResource renderTarget)
        {
            _screenQuadDrawCallData.Textures[0] = sceneRenderTarget.SubResources[0];

            // Pipeline
            _screenQuadDrawCallData.DrawType = DrawType.Indexed;
            _screenQuadDrawCallData.DrawMode = DrawMode.Triangles;
            _screenQuadDrawCallData.IndexedDraw.IndexCount = 6;
            _screenQuadDrawCallData.Shader = shader.NativeShader;
            _screenQuadDrawCallData.Geometry = _screenGeometry;
            _screenQuadDrawCallData.Features = _screenPipelineFeatures;
            _screenQuadDrawCallData.RenderTarget = renderTarget;

            // Iniforms
            _screenQuadDrawCallData.Uniforms[Consts.Graphics.VP_MATRIX_UNIFORM_INDEX].SetMat4(Consts.VIEW_PROJ_UNIFORM_NAME, VP);
            _screenQuadDrawCallData.Uniforms[Consts.Graphics.SCREEN_SIZE_INDEX].SetVec2(Consts.SCREEN_SIZE_UNIFORM_NAME, new vec2(Window.Width, Window.Height));
            _screenQuadDrawCallData.Uniforms[Consts.Graphics.APP_TIME_INDEX].SetVec3(Consts.TIME_UNIFORM_NAME, new vec3(Time.TimeCurrent, Time.TimeCurrent * 2, Time.TimeCurrent * 3));
            _screenQuadDrawCallData.Uniforms[Consts.Graphics.SCREEN_FRAME_BUFFER_GRAB_INDEX].SetInt(Consts.SCREEN_GRAB_TEX_UNIFORM_NAME, 0);

            // Draw
            GfxDeviceManager.Current.Draw(_screenQuadDrawCallData);
        }

        public override void Close()
        {

        }
    }
}