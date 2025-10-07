using Engine.Utils;
using GameCooker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal class AssetDatabaseDevelop : AssetDatabaseBase
    {
        internal override T GetAsset<T>(string path)
        {
            if (GuidPathDict.TryGetByValue(path, out var guid))
            {
                var assetInfo = Database.Assets[guid];

                using (FileStream fs = new FileStream(AssetsCooker.ASSET_DATABASE_ROOT_TEST + "/" + guid + ".bin", FileMode.Open, FileAccess.Read))
                {
                   var encoding = Encoding.Default;

                    using (BinaryReader reader = (assetInfo.Type == AssetType.Text ? new BinaryReader(fs, Encoding.UTF8) : new BinaryReader(fs)))
                    {
                        return BuildAsset(assetInfo, guid, reader) as T;
                    }
                }
            }
            else
            {
                Debug.Error($"Asset doesn't exists at path: {path}");
            }

            return default;
        }
    }
}
