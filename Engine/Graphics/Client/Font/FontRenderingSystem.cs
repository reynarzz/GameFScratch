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
        public ITexture2DManager TextureManager => throw new NotImplementedException();
        private readonly VertexPositionColorTexture[] _vertexData;
        private int _vertexIndex = 0;

        private readonly List<GfxResource> _fontBatches;
        private readonly GfxResource _sharedIndexBuffer;
        private readonly Dictionary<Guid, FontSystem> _fontFamilies;
        private readonly Dictionary<Guid, Texture2D> _textures;

        private readonly DrawCallData _drawCallData;
        private GeometryDescriptor _geometryDescriptor;
        private Shader _testShader;

        public FontRenderingSystem()
        {
            _vertexData = new VertexPositionColorTexture[Consts.Graphics.MAX_FONT_QUADS_PER_BATCH * 4];
            _fontFamilies = new Dictionary<Guid, FontSystem>();

            _fontBatches = new List<GfxResource>();
            _textures = new Dictionary<Guid, Texture2D>();
            _sharedIndexBuffer = GraphicsHelper.CreateQuadIndexBuffer(Consts.Graphics.MAX_FONT_QUADS_PER_BATCH);

            _fontBatches.Add(CreateFontBatchGeometry(ref _geometryDescriptor));

            _drawCallData = new DrawCallData()
            {
                Textures = new GfxResource[5],
                Uniforms = new UniformValue[4],
            };

            _testShader = new Shader(Assets.GetText("Shaders/Font/FontVert.vert").Text, Assets.GetText("Shaders/Font/FontFrag.vert").Text);
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

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
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
            _drawCallData.Viewport = new vec4(0,0, renderTexture.Width, renderTexture.Height);
            _vertexIndex = 0;
        }

        internal void Render(mat4 viewProjection, RenderTexture renderTexture)
        {
            // TODO: refactor, bad performance.
            var fontRenderers = SceneManager.ActiveScene.FindAll<TextRenderer>(findDisabled: false);

            foreach (var fontRenderer in fontRenderers)
            {
                if (!fontRenderer.Font)
                    return;

                if (!_fontFamilies.TryGetValue(fontRenderer.Font.GetID(), out var fontSystem))
                {
                    var settings = new FontSystemSettings
                    {
                        FontResolutionFactor = 2,
                        KernelWidth = 2,
                        KernelHeight = 2
                    };

                    fontSystem = new FontSystem(settings);
                    fontSystem.AddFont(fontRenderer.Font.Data);

                    _fontFamilies.Add(fontRenderer.Font.GetID(), fontSystem);
                }

                var font = fontSystem.GetFont(fontRenderer.FontSize);

                vec2 pivot = new vec2(0, 0);

                float rotation = fontRenderer.Transform.WorldEulerAngles.z;

                var scale = new System.Numerics.Vector2(fontRenderer.Transform.WorldScale.x, fontRenderer.Transform.WorldScale.y);

                var size = font.MeasureString(fontRenderer.Text, scale);
                var origin = new System.Numerics.Vector2(size.X / 2.0f, size.Y / 2.0f);

                // Add all text to render this frame (font components)
                var worldPos = new System.Numerics.Vector2(fontRenderer.Transform.WorldPosition.x, fontRenderer.Transform.WorldPosition.y);
                var color = new FSColor(fontRenderer.Color.R, fontRenderer.Color.G, fontRenderer.Color.B, fontRenderer.Color.A);
                font.DrawText(this, fontRenderer.Text, worldPos, color, rotation, new System.Numerics.Vector2(pivot.x, pivot.y), scale);
            }

            Flush(viewProjection, renderTexture);
        }
    }
}
