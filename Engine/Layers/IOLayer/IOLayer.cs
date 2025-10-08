using Engine.IO;
using GameCooker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class IOLayer : LayerBase
    {
        private static AssetDatabaseBase _assetDatabase;

        public override void Initialize()
        {
            new GameProject().Initialize(new ProjectConfig() { ProjectFolderRoot = "D:/Projects/GameScratch/Game" });

            var cooker = new AssetsCooker();
            _assetDatabase = new AssetDatabaseDevelop();

            var result = cooker.CookAll(new CookOptions() { Type = CookingType.Monolith },
                                                    ProjectPaths.GetAssetsFolderPath(),
                                                    ProjectPaths.GetAssetDatabaseFolder()).Result;
            _assetDatabase.Initialize(result);
        }

        internal static AssetDatabaseBase GetDatabase() // Refactor
        {
            return _assetDatabase;
        }

        public override void Close()
        {
        }

    }
}
