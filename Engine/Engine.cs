using Engine.Graphics;
using Engine.Graphics.OpenGL;
using Engine.Layers;
using Engine.Utils;
using System.Text;

namespace Engine
{
    public class Engine
    {
        private LayersManager _layersManager;

        public Engine()
        {
        }

        public void Initialize(params Type[] layers)
        {
            var win = new Window("Game", 920, 600);

            _layersManager = new LayersManager(layers);
            _layersManager.Initialize();
        }

        public void Run()
        {
            //var geometry = GfxDeviceManager.Current.CreateGeometry(Tests.GetTestGeometryDescriptor());
            //var shader = GfxDeviceManager.Current.CreateShader(Tests.GetTestShaderDescriptor());
            //var texture = GfxDeviceManager.Current.CreateTexture(Tests.TestTextureCreation());

            while (!Window.ShouldClose)
            {
                Window.PollEvents();

                _layersManager.Update();

                //(shader as GLShader).Bind();
                //(geometry as GLGeometry).Bind();
                //(texture as GLTexture).Bind();
                //GfxDeviceManager.Current.DrawIndexed(DrawMode.Triangles, 6);

                Window.SwapBuffers();
            }
        }
    }
}