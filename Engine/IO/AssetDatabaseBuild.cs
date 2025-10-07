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
        protected override T GetAsset<T>(Guid guid, AssetInfo info)
        {
            throw new NotImplementedException();
        }
    }
}