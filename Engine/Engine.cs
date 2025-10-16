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

        public Engine Initialize(string winName, int width, int height, params Type[] layers)
        {
            var win = new Window(winName, width, height);

            if (win.IsInitialized)
            {
                _layersManager = new LayersManager(layers);
                _layersManager.Initialize();
            }

            return this;
        }

        public void Run()
        {
            if (_layersManager == null)
            {
                Debug.Error("FATAL: Engine couldn't not be initialized correctly.");
                return;
            }

            while (!Window.ShouldClose)
            {
                _layersManager.Update();
            }
        }
    }
}