using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.IO
{
    internal class AssetDatabaseDevelop : AssetDatabaseBase
    {
        protected override T GetAsset<T>(Guid guid, AssetInfo assetInfo)
        {
            var binPath = Paths.CreateBinFilePath(Paths.GetAssetDatabaseFolder(), guid.ToString());

            using (FileStream fs = new FileStream(binPath, FileMode.Open, FileAccess.Read))
            {
                var encoding = Encoding.Default;

                if (assetInfo.Type == AssetType.Text)
                {
                    encoding = Encoding.UTF8;
                }

                using (BinaryReader reader = new BinaryReader(fs, encoding))
                {
                    return BuildAsset(assetInfo, guid, reader) as T;
                }
            }
        }
    }
}
