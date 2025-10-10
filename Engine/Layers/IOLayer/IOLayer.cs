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
        private static AssetDatabase _assetDatabase;

        public override void Initialize()
        {
            DiskBase disk = null;
            _assetDatabase = new AssetDatabase();

#if DEBUG
            disk = new DevModeDisk();
#else

#endif
            disk.Initialize();

            _assetDatabase.Initialize(disk);
        }

        internal static AssetDatabase GetDatabase() // Refactor
        {
            return _assetDatabase;
        }

        public override void Close()
        {
        }

    }
}
