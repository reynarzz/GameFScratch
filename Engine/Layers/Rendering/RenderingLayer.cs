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
        private RenderTexture _screenGrabTarget;
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
                Textures = new GfxResource[GfxDeviceManager.Current.GetDeviceInfo().MaxValidTextureUnits],
                Uniforms = new UniformValue[Consts.Graphics.MAX_UNIFORMS_PER_DRAWCALL],
            };

            _screenGrabTarget = new RenderTexture(GfxDeviceManager.Current.CreateRenderTarget(new RenderTargetDescriptor()
            {
                Width = Window.Width,
                Height = Window.Height,
            }), Window.Width, Window.Height);

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

            _screenGrabTarget.UpdateTarget(w, h);
            _defaultSceneRenderTexture.UpdateTarget(w, h);
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
                GfxDeviceManager.Current.SetViewport(new vec4(0, 0, Window.Width, Window.Height));
                GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = new Color(1, 0, 1, 1) });
                return;
            }

            var sceneRenderTarget = _mainCamera.RenderTexture ?? _defaultSceneRenderTexture;
            GfxDeviceManager.Current.SetViewport(new vec4(0, 0, sceneRenderTarget.Width, sceneRenderTarget.Height));

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
                        RenderTarget = _screenGrabTarget.NativeResource
                    });

                    foreach (var batchGrab in batches)
                    {
                        if (!batchGrab.IsActive || batchGrab == batch)
                            break;

                        batchGrab.Flush();
                        RenderPass(batchGrab, ref VP, _screenGrabTarget, _screenGrabTarget, _mainCamera);
                    }
                }

                RenderPass(batch, ref VP, sceneRenderTarget, _screenGrabTarget, _mainCamera);
            }

            Debug.DrawGeometries(VP, sceneRenderTarget.NativeResource);

            foreach (var pass in PostProcessingStack.Passes)
            {
                void PostProcessDraw(Shader shader, RenderTexture inTex, RenderTexture outTex, PostProcessingPass.PassUniform[] uniforms)
                {
                    DrawScreenQuad(shader, VP, inTex, outTex, uniforms, _mainCamera);
                }

                sceneRenderTarget = pass.Render(sceneRenderTarget, PostProcessDraw);
            }

            if (PostProcessingStack.Passes.Count == 0)
            {
                DrawScreenQuad(_screenShader, VP, sceneRenderTarget, null, null, _mainCamera);
            }

            GfxDeviceManager.Current.Present(sceneRenderTarget.NativeResource);
        }

        private void RenderPass(Batch2D batch, ref mat4 VP, RenderTexture renderTarget, RenderTexture screenGrabTarget, Camera camera)
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
                foreach (var (uniformName, texture) in batch.Material.Textures)
                {
                    _drawCallData.NamedTextures[uniformName] = texture.NativeResource;
                }

                int screenGrabIndex = boundTex;

                // Grab the color texture
                _drawCallData.Textures[screenGrabIndex] = pass.IsScreenGrabPass ? screenGrabTarget.NativeResource.SubResources[0] : null;

                // Pipeline
                _pipelineFeatures.Blending = pass.Blending;
                _pipelineFeatures.Stencil = pass.Stencil;

                _drawCallData.DrawType = batch.DrawType;
                _drawCallData.DrawMode = batch.DrawMode;
                _drawCallData.IndexedDraw.IndexCount = batch.IndexCount;
                _drawCallData.Shader = pass.Shader.NativeShader;
                _drawCallData.Geometry = batch.Geometry;
                _drawCallData.Features = _pipelineFeatures;
                _drawCallData.RenderTarget = renderTarget.NativeResource;
                _drawCallData.Viewport = new vec4(0, 0, renderTarget.Width, renderTarget.Height);

                int uniformOffset = 0;
                foreach (var (name, uniform) in pass.Uniforms)
                {
                    _drawCallData.Uniforms[uniformOffset] = uniform;
                    uniformOffset++;
                }

                
                // Iniforms
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.VP_MATRIX].SetMat4(Consts.VIEW_PROJ_UNIFORM_NAME, VP);
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.VIEW_MATRIX].SetMat4(Consts.VIEW_UNIFORM_NAME, camera.ViewMatrix);
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.PROJECTION_MATRIX].SetMat4(Consts.PROJECTION_UNIFORM_NAME, camera.ViewMatrix);
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.TEXTURES_ARRAY].SetIntArr(Consts.TEX_ARRAY_UNIFORM_NAME, Batch2D.TextureSlotArray);
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.MODEL_MATRIX].SetMat4(Consts.MODEL_UNIFORM_NAME, batch.WorldMatrix);
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.SCREEN_RENDER_TARGET_GRAB].SetInt(Consts.SCREEN_GRAB_TEX_UNIFORM_NAME, screenGrabIndex);
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.SCREEN_SIZE].SetVec2(Consts.SCREEN_SIZE_UNIFORM_NAME, new vec2(renderTarget.Width, renderTarget.Height));
                _drawCallData.Uniforms[uniformOffset + (int)Consts.Graphics.Uniforms.APP_TIME].SetVec3(Consts.TIME_UNIFORM_NAME, new vec3(Time.TimeCurrent, Time.TimeCurrent * 2, Time.DeltaTime));

                // Draw
                GfxDeviceManager.Current.Draw(_drawCallData);
            }
        }

        private void DrawScreenQuad(Shader shader, mat4 VP, RenderTexture sceneRenderTarget, RenderTexture renderTarget,
                                    PostProcessingPass.PassUniform[] uniforms, Camera camera)
        {
            _screenQuadDrawCallData.Textures[0] = sceneRenderTarget.NativeResource.SubResources[0];

            int uniformIndex = 0;
            if (uniforms != null)
            {
                for (; uniformIndex < uniforms.Length; uniformIndex++)
                {
                    _screenQuadDrawCallData.Textures[uniformIndex + 1] = uniforms[uniformIndex].RenderTexture.NativeResource.SubResources[0];
                    _screenQuadDrawCallData.Uniforms[uniformIndex].SetInt(uniforms[uniformIndex].Name, uniformIndex + 1);
                }
            }

            // Pipeline
            _screenQuadDrawCallData.DrawType = DrawType.Indexed;
            _screenQuadDrawCallData.DrawMode = DrawMode.Triangles;
            _screenQuadDrawCallData.IndexedDraw.IndexCount = 6;
            _screenQuadDrawCallData.Shader = shader.NativeShader;
            _screenQuadDrawCallData.Geometry = _screenGeometry;
            _screenQuadDrawCallData.Features = _screenPipelineFeatures;
            _screenQuadDrawCallData.RenderTarget = renderTarget?.NativeResource;
            _screenQuadDrawCallData.Viewport = new vec4(0, 0, renderTarget?.Width ?? Window.Width, renderTarget?.Height ?? Window.Height);


            // Uniforms
            _screenQuadDrawCallData.Uniforms[uniformIndex + 0].SetMat4(Consts.VIEW_PROJ_UNIFORM_NAME, VP);
            _screenQuadDrawCallData.Uniforms[uniformIndex + 1].SetVec2(Consts.SCREEN_SIZE_UNIFORM_NAME, new vec2(_screenQuadDrawCallData.Viewport.z, _screenQuadDrawCallData.Viewport.w));
            _screenQuadDrawCallData.Uniforms[uniformIndex + 2].SetVec3(Consts.TIME_UNIFORM_NAME, new vec3(Time.TimeCurrent, Time.TimeCurrent * 2, Time.DeltaTime));
            _screenQuadDrawCallData.Uniforms[uniformIndex + 3].SetInt(Consts.SCREEN_GRAB_TEX_UNIFORM_NAME, 0);
            _screenQuadDrawCallData.Uniforms[uniformIndex + 4].SetFloat(Consts.FRAME_SEED_UNIFORM_NAME, Random.Shared.NextSingle());
            _screenQuadDrawCallData.Uniforms[uniformIndex + 5].SetMat4(Consts.VIEW_UNIFORM_NAME, camera.ViewMatrix);

            // Draw
            GfxDeviceManager.Current.Draw(_screenQuadDrawCallData);
        }

        public override void Close()
        {

        }
    }
}