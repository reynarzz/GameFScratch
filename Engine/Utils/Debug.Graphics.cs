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

        private static int _totalLinesGeometryToDraw = 0;
        private static bool _initializedGraphics = false;


        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct LinesVertex
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

        private static void Initialize()
        {
            if (!_initializedGraphics)
            {
                _initializedGraphics = true;
                _linesGeometry = GraphicsHelper.GetEmptyGeometry<LinesVertex>(LINES_MAX_VERTICES, 0, ref _linesGeoDescriptor);

                _shader = new Shader(DebugVertexShader, DebugFragmentShader);
            }
        }

        public static void DrawRay(vec3 origin, vec3 direction, Color color)
        {
            Initialize();
            // TODO:
        }

        public static void DrawLine(vec3 start, vec3 end, Color color)
        {
            Initialize();
            unsafe
            {
                var offset = sizeof(LinesVertex) * _totalLinesGeometryToDraw;

                var vertices = new LinesVertex[]
                {
                    new LinesVertex() { Position = start, Color = color },
                    new LinesVertex() { Position = end, Color = color }
                };

                var verticesBuffer = MemoryMarshal.AsBytes<LinesVertex>(vertices);

                for (int i = 0; i < verticesBuffer.Length; i++)
                {
                    _linesGeoDescriptor.VertexDesc.BufferDesc.Buffer[offset + i] = verticesBuffer[i];
                }
                
                _linesGeoDescriptor.VertexDesc.BufferDesc.Offset = offset;
            }

            GfxDeviceManager.Current.UpdateGeometry(_linesGeometry, _linesGeoDescriptor);

            _totalLinesGeometryToDraw++;
        }

        public static void DrawBox(vec3 origin, vec3 size, vec3 eulerAngles, Color color)
        {
            Initialize();

        }

        public static void DrawCircle(vec3 origin, float radius, Color color)
        {
            Initialize();

        }

        internal static void DrawGeometries(mat4 ViewProj)
        {
            // Bind shader
            var shader = (_shader.NativeShader as GLShader);
            shader.Bind();
            shader.SetUniform(Consts.VIEW_PROJECTION_UNIFORM_NAME, ViewProj);

            // Draw
            DrawLines();

        }

        private static void DrawLines()
        {
            (_linesGeometry as GLGeometry).Bind();

            GfxDeviceManager.Current.DrawArrays(DrawMode.Lines, 0, _totalLinesGeometryToDraw * 2);
            _totalLinesGeometryToDraw = 0;
        }
    }
}