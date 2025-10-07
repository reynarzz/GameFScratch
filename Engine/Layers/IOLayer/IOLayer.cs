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
            var cooker = new AssetsCooker();
            _assetDatabase = new AssetDatabaseDevelop();

            var result = cooker.CookAll(new CookOptions() { Type = CookingType.Monolith },
                                                    "D:\\Projects\\GameScratch\\Game\\Assets", 
                                                    "D:\\Projects\\GameScratch\\Game\\Library\\AssetsDatabase").Result;
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
