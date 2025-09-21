using Engine.Graphics;
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

            var renderers = SceneManager.ActiveScene.FindAll<Renderer>();
            
            GfxDeviceManager.Current.Clear(new ClearDeviceConfig() { Color = camera.BackgroundColor });

            for (int i = 0; i < renderers.Count; i++)
            {
                // Sort first by sortingOrder, then by transform's z

                var geometry = renderers[i].GetGeometry();


                //GfxDeviceManager.Current.DrawIndexed();
            }
        }
    }
}