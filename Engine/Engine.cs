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

        public Engine Initialize(params Type[] layers)
        {
            var win = new Window("Game", 920, 600);

            if (win.IsInitialized)
            {
                _layersManager = new LayersManager(layers);
                _layersManager.Initialize();
            }

            return this;
        }

        public void Run()
        {
            while (!Window.ShouldClose)
            {
                _layersManager.Update();
            }
        }
    }
}