using Newtonsoft.Json;
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

        public AssetMetaFileBase GetAssetMeta(Guid guid)
        {
            if (AssetDatabaseInfo.Assets.TryGetValue(guid, out var info))
            {
                var bytes = LoadMetaFromDisk(guid);
                var json = Encoding.UTF8.GetString(bytes);

                switch (info.Type)
                {
                    case AssetType.Invalid:
                        break;
                    case AssetType.Texture:
                        return JsonConvert.DeserializeObject<TextureMetaFile>(json);
                    case AssetType.Audio:
                        return JsonConvert.DeserializeObject<AudioMetaFile>(json);
                    case AssetType.Text:
                        return JsonConvert.DeserializeObject<DefaultMetaFile>(json);
                    case AssetType.Shader:
                        return JsonConvert.DeserializeObject<DefaultMetaFile>(json);
                    case AssetType.Font:
                        return JsonConvert.DeserializeObject<DefaultMetaFile>(json);
                    default:
                        throw new NotImplementedException($"Asset type for meta is not implemented: {info.Type}");
                }
            }

            return null;
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
