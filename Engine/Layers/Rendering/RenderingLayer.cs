using Engine.Graphics;
using Engine.Graphics.OpenGL;
using Engine.Rendering;
using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    /*internal*/ public class RenderingLayer : LayerBase
    {
        private List<Renderer> _renderers;

        private Batcher2D _batcher2d;
        private Camera _mainCamera = null;

        public RenderingLayer()
        {
            _batcher2d = new Batcher2D(777);
        }

        public override void Initialize()
        {
        }

        public override void Close()
        {
        }

        public  mat4 Mul(mat4 lhs, mat4 rhs)
        {
            mat4 r = new mat4();

            r.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
            r.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
            r.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
            r.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;

            r.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
            r.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
            r.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
            r.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;

            r.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
            r.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
            r.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
            r.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;

            r.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
            r.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
            r.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
            r.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;

            return r;
        }

        internal override void UpdateLayer()
        {
            if (!_mainCamera)
            {
                _mainCamera = SceneManager.ActiveScene.Find<Camera>();
            }

            if (!_mainCamera || !_mainCamera.IsEnabled)
            {
                Log.Error("No cameras found in scene.");
                GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = new GlmSharp.vec4(1, 0, 1, 1) });
                return;
            }

            // Clear screen
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = _mainCamera.BackgroundColor });

            var batches = _batcher2d.CreateBatches(SceneManager.ActiveScene.FindAll<Renderer2D>());

             var VP = Mul(_mainCamera.Projection, _mainCamera.ViewMatrix);
            
            foreach (var batch in batches)
            {
                batch.Flush();
                (batch.Geometry as GLGeometry).Bind();
                var shader = batch.Material.Shader.NativeShader as GLShader;
                shader.SetUniform("uVP", VP);

                for (int i = 0; i < batch.Textures.Length; i++) 
                {
                    var tex = (batch.Textures[i]);

                    if (tex == null)
                        break;

                    (tex.InternalTexture as GLTexture).Bind(i);
                }

                shader.SetUniform("uTextures", Batch2D.TextureSlotArray);
                shader.Bind();

                // TODO: Bind these
                // batch.Geometry;
                // batch.Material;
                // batch.Textures;

                // Draw
                 GfxDeviceManager.Current.DrawIndexed(DrawMode.Triangles, batch.IndexCount);
            }
        }
    }
}