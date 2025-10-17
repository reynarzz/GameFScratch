using Engine.Rendering;
using Engine.Utils;
using FontStashSharp;
using FontStashSharp.Interfaces;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    internal class FontRenderingSystem : IFontStashRenderer2
    {
        private FontTextureManager _textureManager;
        ITexture2DManager IFontStashRenderer2.TextureManager => _textureManager;

        private readonly VertexPositionColorTexture[] _vertexData;
        private int _vertexIndex = 0;

        private readonly List<GfxResource> _fontBatches;
        private readonly GfxResource _sharedIndexBuffer;
        private readonly Dictionary<Guid, FontSystem> _fontFamilies;
        private readonly Dictionary<Guid, Texture2D> _textures;

        private readonly DrawCallData _drawCallData;
        private GeometryDescriptor _geometryDescriptor;
        private Shader _testShader;
        private mat4 _viewMatrix;

        public FontRenderingSystem()
        {
            _vertexData = new VertexPositionColorTexture[Consts.Graphics.MAX_FONT_QUADS_PER_BATCH * 4];
            _fontFamilies = new Dictionary<Guid, FontSystem>();
            _textureManager = new FontTextureManager();

            _fontBatches = new List<GfxResource>();
            _textures = new Dictionary<Guid, Texture2D>();
            _sharedIndexBuffer = GraphicsHelper.CreateQuadIndexBuffer(Consts.Graphics.MAX_FONT_QUADS_PER_BATCH);

            _fontBatches.Add(CreateFontBatchGeometry(ref _geometryDescriptor));

            _drawCallData = new DrawCallData()
            {
                Textures = new GfxResource[5],
                Uniforms = new UniformValue[4],
            };

            _testShader = new Shader(Assets.GetText("Shaders/Font/FontVert.vert").Text, 
                                     Assets.GetText("Shaders/Font/FontFrag.frag").Text);

            _drawCallData.Features = new PipelineFeatures();
            _drawCallData.Features.Blending.Enabled = true;
            _drawCallData.Features.Blending.SrcFactor = BlendFactor.SrcAlpha;
            _drawCallData.Features.Blending.DstFactor = BlendFactor.OneMinusSrcAlpha;
            _drawCallData.Features.Blending.Equation = BlendEquation.FuncAdd;

            _viewMatrix = MathUtils.Ortho(0, Window.Width, Window.Height, 0, 0, -1);
        }

        private GfxResource CreateFontBatchGeometry(ref GeometryDescriptor desc)
        {
            unsafe
            {
                var stride = sizeof(VertexPositionColorTexture);
                var attribs = new VertexAtrib[]
                {
                     new VertexAtrib() { Count = 3, Normalized = false, Type = GfxValueType.Float, Stride = stride, Offset = 0 }, // Position
                     new VertexAtrib() { Count = 4, Normalized = true,  Type = GfxValueType.UByte, Stride = stride, Offset = sizeof(float) * 3 }, // Color
                     new VertexAtrib() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = stride, Offset = sizeof(float) * 3 + 4 }, // UV
                };

                return GraphicsHelper.GetEmptyGeometry(_vertexData.Length, 0, ref desc, attribs, _sharedIndexBuffer);
            }
        }

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, 
                                             ref VertexPositionColorTexture topRight, 
                                             ref VertexPositionColorTexture bottomLeft, 
                                             ref VertexPositionColorTexture bottomRight)
        {
            var tex = texture as Texture2D;

            _textures[tex.GetID()] = tex;

            if (_vertexData.Length > _vertexIndex + 4)
            {
                _vertexData[_vertexIndex++] = bottomLeft;
                _vertexData[_vertexIndex++] = topLeft;
                _vertexData[_vertexIndex++] = topRight;
                _vertexData[_vertexIndex++] = bottomRight;
            }
        }


        public void Flush(mat4 viewProjection, RenderTexture renderTexture)
        {
            // Call draw call here

            // Bind shaders
            // Bind geometries.

            var geometryTest = _fontBatches[0];

            _geometryDescriptor.VertexDesc.BufferDesc.Offset = 0;
            unsafe
            {
                _geometryDescriptor.VertexDesc.BufferDesc.Count = sizeof(VertexPositionColorTexture) * _vertexIndex;
            }

            // TODO: improve this, it should not be called every frame.
            _geometryDescriptor.VertexDesc.BufferDesc.Buffer = MemoryMarshal.AsBytes<VertexPositionColorTexture>(_vertexData).ToArray();

            GfxDeviceManager.Current.UpdateResouce(geometryTest, _geometryDescriptor);
            int texIndex = 0;
            foreach (var (guid, texture) in _textures)
            {
                var tex = texture;
                _drawCallData.Textures[texIndex] = texture.NativeResource;

                texIndex++;
            }

            _drawCallData.DrawMode = DrawMode.Triangles;
            _drawCallData.DrawType = DrawType.Indexed;
            _drawCallData.Geometry = geometryTest;
            _drawCallData.Shader = _testShader.NativeShader;
            _drawCallData.RenderTarget = renderTexture.NativeResource;
            _drawCallData.IndexedDraw.IndexCount = _vertexIndex * 6;
            _drawCallData.Viewport = new vec4(0, 0, renderTexture.Width, renderTexture.Height);

            _drawCallData.Uniforms[0].SetMat4(Consts.VIEW_PROJ_UNIFORM_NAME, _viewMatrix);
            _drawCallData.Uniforms[1].SetIntArr(Consts.TEX_ARRAY_UNIFORM_NAME, Batch2D.TextureSlotArray);

            GfxDeviceManager.Current.Draw(_drawCallData);
            _vertexIndex = 0;
        }

        internal void Render(mat4 viewProjection, RenderTexture renderTexture)
        {
            // TODO: refactor, bad performance.
            var fontRenderers = SceneManager.ActiveScene.FindAll<TextRenderer>(findDisabled: false);

            foreach (var textRenderer in fontRenderers)
            {
                if (!textRenderer.Font)
                    return;

                if (!_fontFamilies.TryGetValue(textRenderer.Font.GetID(), out var fontSystem))
                {
                    var settings = new FontSystemSettings
                    {
                        FontResolutionFactor = 3,
                        KernelWidth = 2,
                        KernelHeight = 2,
                    };

                    fontSystem = new FontSystem(settings);
                    fontSystem.AddFont(textRenderer.Font.Data);

                    _fontFamilies.Add(textRenderer.Font.GetID(), fontSystem);
                }

                var font = fontSystem.GetFont(textRenderer.FontSize);

                var pivot = new System.Numerics.Vector2();

                float rotation = glm.radians(textRenderer.Transform.WorldEulerAngles.z);

                var scale = new System.Numerics.Vector2(textRenderer.Transform.WorldScale.x, 
                                                        textRenderer.Transform.WorldScale.y);

                var size = font.MeasureString(textRenderer.Text, scale);
                var origin = new System.Numerics.Vector2(size.X / 2.0f, size.Y / 2.0f);

                var worldPos = new System.Numerics.Vector2(textRenderer.Transform.WorldPosition.x, 
                                                           textRenderer.Transform.WorldPosition.y);
                var color = new FSColor(textRenderer.Color.R, 
                                        textRenderer.Color.G, 
                                        textRenderer.Color.B, 
                                        textRenderer.Color.A);
                var effect = textRenderer.OutlineSize > 0 ? FontSystemEffect.Stroked : FontSystemEffect.None;

                font.DrawText(this, textRenderer.Text, worldPos, color, rotation, pivot, scale,0, 
                              textRenderer.CharacterSpacing, textRenderer.LineSpacing, TextStyle.None, 
                              effect, Math.Clamp(textRenderer.OutlineSize, 0, textRenderer.OutlineSize + 1));
            }

            Flush(viewProjection, renderTexture);
        }
    }
}
