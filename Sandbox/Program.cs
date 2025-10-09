using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Engine;
using Engine.Layers;

using Game;
using GameCooker;
using SharedTypes;

namespace Sandbox
{
    internal class Program
    {
        const string GAME_FOLDER_NAME = "Game";

        private static void Main()
        {

#if RELEASE
            var libsPath = Path.Combine(AppContext.BaseDirectory, "Data/Assemblies");

            foreach (var dll in Directory.GetFiles(libsPath, "*.dll"))
            {
                try
                {
                    NativeLibrary.Load(dll);
                }
                catch { }
            }
#else
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "..", GAME_FOLDER_NAME));

            new GameProject().Initialize(new ProjectConfig() { ProjectFolderRoot = path });
            new AssetsCooker().CookAll(new CookOptions() { Type = CookingType.DevMode },
                                       Paths.GetAssetsFolderPath(),
                                       Paths.GetAssetDatabaseFolder());
#endif

            var engine = new Engine.Engine();
            engine.Initialize(typeof(TimeLayer),
                              typeof(Input),
                              typeof(GameApplication),
                              typeof(SceneLayer),
                              typeof(PhysicsLayer),
                              typeof(RenderingLayer),
                              typeof(IOLayer));

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
