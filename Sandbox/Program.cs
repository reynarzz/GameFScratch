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
        private const string GAME_FOLDER_NAME = "Game";

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
            // This will import all the assets without using the GUI tool. Useful for recruiters needing to just run the project.
            var assemblyDir = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            var path = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "..", GAME_FOLDER_NAME));

            new GameProject().Initialize(new ProjectConfig() { ProjectFolderRoot = path });
            new AssetsCooker().CookAll(new CookOptions() { Type = CookingType.DevMode },
                                       Paths.GetAssetsFolderPath(),
                                       Paths.GetAssetDatabaseFolder());
#endif
            new Engine.Engine().Initialize(typeof(TimeLayer),
                                           typeof(Input),
                                           typeof(GameApplication),
                                           typeof(MainThreadDispatcher),
                                           typeof(SceneLayer),
                                           typeof(PhysicsLayer),
                                           typeof(RenderingLayer),
                                           typeof(IOLayer))
                                          .Run();
        }
    }
}