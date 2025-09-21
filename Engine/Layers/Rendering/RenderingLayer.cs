using Engine.Graphics;
using Engine.Rendering;
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

        internal override void UpdateLayer()
        {
            var camera = SceneManager.ActiveScene.Find<Camera>();

            if (!camera)
            {
                Log.Error("No cameras found in scene.");

                GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = new GlmSharp.vec4(1, 0, 1, 1) });
                return;
            }

            // Clear screen
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = camera.BackgroundColor });

            var spriteRenderers = SceneManager.ActiveScene.FindAll<SpriteRenderer>();

            var batches = _batcher2d.CreateBatches(spriteRenderers);

            for (int i = 0; i < batches.Count; i++)
            {
                // Sort first by sortingOrder, then by transform's z

                // - Multiply the four vertex of quad with the worldMatrix
                // spriteRenderers[i].Transform.WorldMatrix;
                // Geometries will be created by the batcher
                // var geometry = renderers[i].GetGeometry();
                

                //GfxDeviceManager.Current.DrawIndexed();
            }
        }
    }
}