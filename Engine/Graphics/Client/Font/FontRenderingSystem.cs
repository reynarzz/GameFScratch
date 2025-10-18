using Engine.Rendering;
using Engine.Utils;
using FontStashSharp;
using FontStashSharp.Interfaces;
using GLFW;
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct FontVertex
        {
            public vec2 Position;
            public vec2 UV;
            public uint Color;
            public int VertexIndex;
        }

        private readonly FontVertex[] _vertexData;
        private int _vertexIndex = 0;

        private readonly List<GfxResource> _fontBatches;
        private readonly GfxResource _sharedIndexBuffer;
        private readonly Dictionary<Guid, FontSystem> _fontFamilies;
        private readonly Dictionary<Guid, Texture2D> _textures;

        private readonly DrawCallData _drawCallData;
        private GeometryDescriptor _geometryDescriptor;
        private Shader _testShader;
        private mat4 _viewMatrix;
        private vec2 _targetScreenRes;

        public FontRenderingSystem()
        {
            _vertexData = new FontVertex[Consts.Graphics.MAX_FONT_QUADS_PER_BATCH * 4];
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

            //_targetScreenRes = new vec2(Window.Width, Window.Height);
            _targetScreenRes = new vec2(512 * 2, 288 * 2);
            //_viewMatrix = MathUtils.Ortho(0, _targetScreenRes.x, _targetScreenRes.y, 0, 0, 1);
            _viewMatrix = MathUtils.Ortho(-_targetScreenRes.x / 2, _targetScreenRes.x / 2, _targetScreenRes.y / 2, -_targetScreenRes.y / 2, 0, -1);
        }

        private GfxResource CreateFontBatchGeometry(ref GeometryDescriptor desc)
        {
            unsafe
            {
                var stride = sizeof(FontVertex);
                var attribs = new VertexAtrib[]
                {
                     new VertexAtrib() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = stride, Offset = 0 }, // Position
                     new VertexAtrib() { Count = 2, Normalized = false, Type = GfxValueType.Float, Stride = stride, Offset = sizeof(float) * 2 }, // UV
                     new VertexAtrib() { Count = 1, Normalized = false, Type = GfxValueType.Uint, Stride = stride, Offset = sizeof(float) * 4 }, // Color
                     new VertexAtrib() { Count = 1, Normalized = false, Type = GfxValueType.Int, Stride = stride, Offset = sizeof(float) * 5 }, // charIndex
                };

                return GraphicsHelper.GetEmptyGeometry<FontVertex>(_vertexData.Length, 0, ref desc, attribs, _sharedIndexBuffer);
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
                SetFontVertex(_vertexData, ref bottomLeft, _vertexIndex + 0);
                SetFontVertex(_vertexData, ref topLeft, _vertexIndex + 1);
                SetFontVertex(_vertexData, ref topRight, _vertexIndex + 2);
                SetFontVertex(_vertexData, ref bottomRight, _vertexIndex + 3);

                _vertexIndex += 4;
            }
        }

        private void SetFontVertex(FontVertex[] fVertex, ref VertexPositionColorTexture vertex, int vertexIndex)
        {
            fVertex[vertexIndex].Position = new vec2(vertex.Position.X, vertex.Position.Y);
            fVertex[vertexIndex].UV = new vec2(vertex.TextureCoordinate.X, vertex.TextureCoordinate.Y);
            fVertex[vertexIndex].Color = vertex.Color.PackedValue;
            fVertex[vertexIndex].VertexIndex = vertexIndex;
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
                _geometryDescriptor.VertexDesc.BufferDesc.Count = sizeof(FontVertex) * _vertexIndex;
            }

            (_geometryDescriptor.VertexDesc.BufferDesc as BufferDataDescriptor<FontVertex>).Buffer =_vertexData;

            GfxDeviceManager.Current.UpdateResouce(geometryTest, _geometryDescriptor);
            int texIndex = 0;
            foreach (var (guid, texture) in _textures)
            {
                var tex = texture;
                _drawCallData.Textures[texIndex] = texture.NativeResource;

                texIndex++;
            }

            _drawCallData.Features.Blending.Enabled = true;
            _drawCallData.Features.Blending.SrcFactor = BlendFactor.SrcAlpha;
            _drawCallData.Features.Blending.DstFactor = BlendFactor.OneMinusSrcAlpha;
            _drawCallData.Features.Blending.Equation = BlendEquation.FuncAdd;


            _drawCallData.DrawMode = DrawMode.Triangles;
            _drawCallData.DrawType = DrawType.Indexed;
            _drawCallData.Geometry = geometryTest;
            _drawCallData.Shader = _testShader.NativeShader;
            _drawCallData.RenderTarget = renderTexture.NativeResource;
            _drawCallData.IndexedDraw.IndexCount = _vertexIndex * 6;
            _drawCallData.Viewport = new vec4(0, 0, renderTexture.Width, renderTexture.Height);

            _drawCallData.Uniforms[0].SetMat4(Consts.VIEW_PROJ_UNIFORM_NAME, _viewMatrix);
            _drawCallData.Uniforms[1].SetIntArr(Consts.TEX_ARRAY_UNIFORM_NAME, Batch2D.TextureSlotArray);
            _drawCallData.Uniforms[2].SetVec3(Consts.TIME_UNIFORM_NAME, new vec3(Time.TimeCurrent, Time.TimeCurrent * 2, Time.DeltaTime));

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

                // TODO: remove this, used for recentering
                var split = textRenderer.Text.ToString().Split('\n');

                for (int i = 0; i < split.Length; i++)
                {
                    SendTextToDraw(split[i], font, textRenderer, font.LineHeight * i);
                }
            }

            Flush(viewProjection, renderTexture);
        }

        private void SendTextToDraw(string text, DynamicSpriteFont font, TextRenderer textRenderer, int lineHeight)
        {
            var pivot = new System.Numerics.Vector2(0.5f, 0.5f);

            float rotation = glm.radians(textRenderer.Transform.WorldEulerAngles.z);

            var scale = new System.Numerics.Vector2(textRenderer.Transform.WorldScale.x,
                                                    textRenderer.Transform.WorldScale.y);

            var effect = textRenderer.OutlineSize > 0 ? FontSystemEffect.Stroked : FontSystemEffect.None;

            var size = font.MeasureString(text, scale, textRenderer.CharacterSpacing, textRenderer.LineSpacing, effect, textRenderer.OutlineSize);
            var origin = new System.Numerics.Vector2(size.X, size.Y);
            origin = default;
            var position = new System.Numerics.Vector2(textRenderer.Transform.WorldPosition.x,
                                                       textRenderer.Transform.WorldPosition.y + lineHeight);

            var bounds = font.TextBounds(text, position, scale, textRenderer.CharacterSpacing, textRenderer.LineSpacing, effect, textRenderer.OutlineSize);

            var textSize = new System.Numerics.Vector2(bounds.X2 - bounds.X, bounds.Y2 - bounds.Y);

            // Compute pivot offset
            var pivotOffset = new System.Numerics.Vector2(textSize.X * pivot.X, textSize.Y * pivot.Y);

            // Final position to start rendering from
            var finalPosition = position - pivotOffset;

            // This line calls the DrawQuad function for every character.
            font.DrawText(this, text, finalPosition, new FSColor(textRenderer.Color), rotation, origin, scale, 0,
                          textRenderer.CharacterSpacing, textRenderer.LineSpacing, TextStyle.None,
                          effect, Math.Clamp(textRenderer.OutlineSize, 0, textRenderer.OutlineSize + 1));
        }
    }
}
