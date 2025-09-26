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
            engine.Initialize(typeof(TimeLayer),
                              typeof(Input),
                              typeof(GameApplication), 
                              typeof(SceneLayer),
                              typeof(PhysicsLayer),
                              typeof(RenderingLayer),
                              typeof(EndFrameLayer));

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
