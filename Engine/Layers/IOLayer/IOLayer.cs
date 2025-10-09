using Engine.IO;
using Newtonsoft.Json;
using SharedTypes;
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
#if DEBUG
            _assetDatabase = new AssetDatabaseDevelop();
#else

#endif
            var testInfo = JsonConvert.DeserializeObject<AssetsDatabaseInfo>(File.ReadAllText(Paths.GetAssetDatabaseFilePath()));
            _assetDatabase.Initialize(testInfo);
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
