using System.Reflection;
using Engine;
using Engine.Layers;

using Game;

namespace Sandbox
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var engine = new Engine.Engine();
            engine.Initialize(typeof(GameApplication), 
                              typeof(SceneLayer), 
                              typeof(RenderingLayer));

            engine.Run();

//#if DEBUG
//            try
//            {
//                engine.Run();
//            }
//            catch (Exception e)
//            {
//                Log.Error(e);
//            }
//#else
//            engine.Run();
//#endif
        }
    }
}
