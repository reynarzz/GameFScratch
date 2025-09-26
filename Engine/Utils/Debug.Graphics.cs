using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Graphics.OpenGL;
using Engine.Utils;
using GlmNet;

namespace Engine
{
    public static partial class Debug
    {
        private static GfxResource _linesGeometry;
        private static GeometryDescriptor _linesGeoDescriptor;
        private static Shader _shader;

        private const int LINES_MAX_VERTICES = 10000;
        private const float CIRCLE_MAX_SEGMENTS = 15;

        private static int _totalLinesVerticesToDraw = 0;
        private static bool _initializedGraphics = false;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct DebugVertex
        {
            public vec3 Position;
            public ColorPacketRGBA Color;
        }

        private static string DebugVertexShader = @"
            #version 330 core
            layout(location = 0) in vec3 position;
            layout(location = 1) in uint color; 
        
            out vec4 vColor;
            uniform mat4 uVP;
        
            vec4 unpackColor(uint c) 
            {
                float r = float((c >> 24) & 0xFFu) / 255.0;
                float g = float((c >> 16) & 0xFFu) / 255.0;
                float b = float((c >>  8) & 0xFFu) / 255.0;
                float a = float( c        & 0xFFu) / 255.0;
                return vec4(r,g,b,a);
            }
        
            void main() 
            {
                vColor = unpackColor(color);
                gl_Position = uVP * vec4(position, 1.0);
            }
        ";

        private static string DebugFragmentShader = $@"
            #version 330 core
        
            in vec4 vColor;
        
            out vec4 fragColor;
        
            void main()
            {{
                fragColor = vColor;
            }}
        ";


        private static DebugVertex[] _linesVertexPositions;

        private static void Initialize()
        {
            if (!_initializedGraphics)
            {
                _initializedGraphics = true;
                _linesGeometry = GraphicsHelper.GetEmptyGeometry<DebugVertex>(LINES_MAX_VERTICES, 0, ref _linesGeoDescriptor);

                _shader = new Shader(DebugVertexShader, DebugFragmentShader);
                _linesVertexPositions = new DebugVertex[LINES_MAX_VERTICES];
            }
        }

        public static void DrawRay(vec3 origin, vec3 direction, Color color)
        {
            Initialize();
            DrawLine(origin, origin + direction, color);
        }

        public static void DrawLine(vec3 start, vec3 end, Color color)
        {
            Initialize();

            if(_totalLinesVerticesToDraw >= _linesVertexPositions.Length)
            {
                Debug.Error($"Can't draw more lines, lines vertices max is: {LINES_MAX_VERTICES}");
                return;
            }
            _linesVertexPositions[_totalLinesVerticesToDraw + 0] = new DebugVertex() { Position = start, Color = color };
            _linesVertexPositions[_totalLinesVerticesToDraw + 1] = new DebugVertex() { Position = end, Color = color };

            _totalLinesVerticesToDraw += 2;
        }

        public static void DrawBox(vec3 origin, vec3 size, vec3 eulerAngles, Color color)
        {
            Initialize();

        }

        public static void DrawCircle(vec3 origin, float radius, Color color)
        {
            Initialize();
           
            float angleStep = 2.0f * MathF.PI / CIRCLE_MAX_SEGMENTS;

            for (int i = 0; i < CIRCLE_MAX_SEGMENTS; i++)
            {
                float a0 = i * angleStep;
                float a1 = (i + 1) * angleStep;

                var p0 = new vec3(origin.x + MathF.Cos(a0) * radius, origin.y + MathF.Sin(a0) * radius, origin.z);
                var p1 = new vec3(origin.x + MathF.Cos(a1) * radius, origin.y + MathF.Sin(a1) * radius, origin.z);

                DrawLine(p0, p1, color);
            }
        }

        internal static void DrawGeometries(mat4 ViewProj)
        {
            if (_initializedGraphics)
            {
                // Bind shader
                var shader = (_shader.NativeShader as GLShader);
                shader.Bind();
                shader.SetUniform(Consts.VIEW_PROJECTION_UNIFORM_NAME, ViewProj);

                // Push geometries updates
                PushLineGeometries();

                // Draw
                DrawLines();
            }
        }

        private static void DrawLines()
        {
            (_linesGeometry as GLGeometry).Bind();

            GfxDeviceManager.Current.DrawArrays(DrawMode.Lines, 0, _totalLinesVerticesToDraw);
            _totalLinesVerticesToDraw = 0;
        }

        private static void PushLineGeometries()
        {
            unsafe
            {
                var verticesBuffer = MemoryMarshal.AsBytes<DebugVertex>(_linesVertexPositions);

                // Only copy the vertices needed.
                for (int i = 0; i < sizeof(DebugVertex) * _totalLinesVerticesToDraw; i++)
                {
                    _linesGeoDescriptor.VertexDesc.BufferDesc.Buffer[i] = verticesBuffer[i];
                }

                _linesGeoDescriptor.VertexDesc.BufferDesc.Offset = 0;
                _linesGeoDescriptor.VertexDesc.BufferDesc.Count = _totalLinesVerticesToDraw;
            }

            GfxDeviceManager.Current.UpdateGeometry(_linesGeometry, _linesGeoDescriptor);
        }
    }
}