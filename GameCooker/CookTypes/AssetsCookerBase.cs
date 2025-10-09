using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    internal abstract class AssetsCookerBase
    {
        internal abstract Task CookAssetsAsync(IEnumerable<(string, AssetType)> files, 
                                                              Func<AssetType, string, byte[]> processAssetCallback, 
                                                              AssetsDatabaseInfo database, string outFolder);
    }
}