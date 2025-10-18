using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;

namespace Engine.Utils
{
    internal class Tests
    {
       

        internal readonly static string VertexShaderPink = @"
          #version 330 core

        layout(location = 0) in vec3 position;
        layout(location = 1) in vec2 uv;

        out vec2 fragUV;
        uniform mat4 uVP;

        void main() 
        {
            fragUV = uv;
            gl_Position = uVP * vec4(position, 1.0);
        }
        ";

        internal readonly static string QuadVertexShader = @"
          #version 330 core

        layout(location = 0) in vec3 position;
        layout(location = 1) in vec2 uv;

        out vec2 fragUV;

        void main() 
        {
            fragUV = uv;
            gl_Position = vec4(position, 1.0);
        }
        ";

        internal readonly static string QuadFragmentShader = @"
           #version 330 core
            in vec2 fragUV;
            out vec4 color;

            uniform sampler2D uTexture; // uniform for the texture

            void main()
            {
                color = texture(uTexture, fragUV);
            }";

        private static string fragment = @"
       #version 330 core

        out vec4 color;

        void main()
        {
            color = vec4(1.0, 1.0, 1.0, 1.0); 
        }";

        private static string fragmentPink = @"
       #version 330 core

        out vec4 color;

        void main()
        {
            color = vec4(1.0, 0.0, 1.0, 1.0); 
        }";

       
        internal static Shader GetShaderPink()
        {
            return new Shader(VertexShaderPink, fragmentPink);
        }


        internal static ShaderDescriptor GetTestShaderDescriptor()
        {
            var shaderDescriptor = new ShaderDescriptor();
            shaderDescriptor.VertexSource = Encoding.UTF8.GetBytes(QuadVertexShader);
            shaderDescriptor.FragmentSource = Encoding.UTF8.GetBytes(QuadFragmentShader);
            return shaderDescriptor;
        }


        internal static TextureDescriptor TestTextureCreation()
        {
            var textureDescriptor = new TextureDescriptor();
            textureDescriptor.Width = 96;
            textureDescriptor.Height = 96;

            var bufferSize = textureDescriptor.Width * textureDescriptor.Height * 4;
            textureDescriptor.Buffer = new byte[bufferSize];

            new Random().NextBytes(textureDescriptor.Buffer);

            return textureDescriptor;
        }

        internal static void TestPolygonSubDivider()
        {
            List<Polygon.PVertex> verts = new List<Polygon.PVertex>
            {
                new Polygon.PVertex(new Polygon.Vec2(0, 0)),
                new Polygon.PVertex(new Polygon.Vec2(4, 1)),
                new Polygon.PVertex(new Polygon.Vec2(8, 0)),
                new Polygon.PVertex(new Polygon.Vec2(6, 3)),
                new Polygon.PVertex(new Polygon.Vec2(8, 6)),
                new Polygon.PVertex(new Polygon.Vec2(4, 5)),
                new Polygon.PVertex(new Polygon.Vec2(0, 6)),
                new Polygon.PVertex(new Polygon.Vec2(2, 3)),
                new Polygon.PVertex(new Polygon.Vec2(1, 2)),
                new Polygon.PVertex(new Polygon.Vec2(3, 2))
            };

            Polygon.ConcavePolygon poly = new Polygon.ConcavePolygon(verts);

            Debug.Info("Original vertices:");
            for (int i = 0; i < poly.GetPointCount(); i++)
            {
                Polygon.Vec2 p = poly.GetPoint(i);
                Debug.Info($"  {i}: ({p.x}, {p.y})");
            }

            // Ear-clipping decomposition
            poly.ConvexDecomp();
            List<Polygon.ConcavePolygon> earPieces = new List<Polygon.ConcavePolygon>();
            poly.ReturnLowestLevelPolys(earPieces, 7);

            Debug.Info($"\nEar-clipping decomposition: {earPieces.Count} convex pieces");
            for (int i = 0; i < earPieces.Count; i++)
            {
                Debug.Info($"Piece {i}:");
                for (int j = 0; j < earPieces[i].GetPointCount(); j++)
                {
                    Polygon.Vec2 p = earPieces[i].GetPoint(j);
                    Debug.Info($"  ({p.x}, {p.y})");
                }
            }

            // Bayazit decomposition
            poly.ConvexDecompBayazit();
            List<Polygon.ConcavePolygon> bayazitPieces = new List<Polygon.ConcavePolygon>();
            poly.ReturnLowestLevelPolys(bayazitPieces, 7);

            Debug.Info($"\nBayazit decomposition: {bayazitPieces.Count} convex pieces");
            for (int i = 0; i < bayazitPieces.Count; i++)
            {
                Debug.Info($"Piece {i}:");
                for (int j = 0; j < bayazitPieces[i].GetPointCount(); j++)
                {
                    Polygon.Vec2 p = bayazitPieces[i].GetPoint(j);
                    Debug.Info($"  ({p.x}, {p.y})");
                }
            }

            Debug.Info("\nDone.");
        }
    }
}
