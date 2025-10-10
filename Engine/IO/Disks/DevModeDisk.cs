using Newtonsoft.Json;
using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal class DevModeDisk : DiskBase
    {
        public override bool Initialize()
        {
            AssetDatabaseInfo = JsonConvert.DeserializeObject<AssetsDatabaseInfo>(File.ReadAllText(Paths.GetAssetDatabaseFilePath()));

            return AssetDatabaseInfo != null;
        }

        protected override byte[] LoadAssetFromDisk(Guid guid)
        {
            return File.ReadAllBytes(Paths.CreateBinFilePath(Paths.GetAssetDatabaseFolder(), guid.ToString()));
        }

        protected override async Task<byte[]> LoadAssetFromDiskAsync(Guid guid)
        {
            return await File.ReadAllBytesAsync(Paths.CreateBinFilePath(Paths.GetAssetDatabaseFolder(), guid.ToString()));
        }

        protected override byte[] LoadMetaFromDisk(Guid guid)
        {
            if(AssetDatabaseInfo.Assets.TryGetValue(guid, out var assetInfo))
            {
                return File.ReadAllBytes(Paths.GetAbsoluteAssetPath(assetInfo.Path));
            }
            return null;
        }
    }
}