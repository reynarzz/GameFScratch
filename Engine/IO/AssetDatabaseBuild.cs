using Engine.Utils;
using GameCooker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal class AssetDatabaseBuild : AssetDatabaseBase
    {
        private BiDictionary<string, string> _guidPathDict;

        internal override T GetAsset<T>(string path)
        {
            return default;
        }

        internal override void Initialize(AssetsDatabaseInfo database)
        {
           
        }
    }
}
