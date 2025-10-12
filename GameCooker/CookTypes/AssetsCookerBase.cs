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
        internal abstract Task CookAssetsAsync(CookFileOptions fileOptions, (string, AssetType)[] files, 
                                               Func<AssetType, AssetMetaFileBase, string, byte[]> processAssetCallback, 
                                               string outFolder);
    }
}