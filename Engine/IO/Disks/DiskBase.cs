using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal abstract class DiskBase
    {
        public AssetsDatabaseInfo AssetDatabaseInfo { get; protected set; } = new();
        protected Dictionary<Guid, byte[]> AssetsData { get; private set; } = new();

        public abstract bool Initialize();

        public struct AssetContent
        {
            public AssetInfo Info { get; set; }
            public byte[] RawData { get; set; }

            public bool Success => RawData != null;
        }

        public async Task<AssetContent> GetAssetAsync(Guid guid)
        {
            if (AssetDatabaseInfo.Assets.TryGetValue(guid, out var info))
            {
                if (AssetsData.TryGetValue(guid, out var data))
                {
                    return new AssetContent() { Info = info, RawData = data };
                }

                data = await LoadAssetFromDiskAsync(guid);

                if(data == null)
                {
                    Debug.Error("Fatal: Can't load asset from disk, is in database table but contents are not in disk?");
                    return default;
                }
                AssetsData.Add(guid, data);

                return new AssetContent() { Info = info, RawData = data };
            }

            return default;
        }

        protected abstract Task<byte[]> LoadAssetFromDiskAsync(Guid guid);
        protected abstract byte[] LoadAssetFromDisk(Guid guid);
        protected abstract byte[] LoadMetaFromDisk(Guid guid);

        public void ReleaseAsset(Guid guid)
        {
            AssetsData.Remove(guid);
        }
    }
}
