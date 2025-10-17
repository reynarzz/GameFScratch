using Engine.Utils;
using SharedTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal class AssetDatabase
    {
        private readonly Dictionary<AssetType, AssetBuilderBase> _assetbuilder;
        private BiDictionary<Guid, string> _guidPathDict;
        private DiskBase _disk;
        private DiskBase Disk => _disk;
        public AssetDatabase()
        {
            _assetbuilder = new()
            {
                { AssetType.Texture, new TextureAssetBuilder() },
                { AssetType.Text, new TextAssetBuilder() },
                { AssetType.Shader, new TextAssetBuilder() },
                { AssetType.Audio, new AudioClipAssetBuilder() },
                { AssetType.Font, new FontAssetBuilder() },
            };
        }

        internal virtual void Initialize(DiskBase disk)
        {
            _disk = disk;
            _guidPathDict = new BiDictionary<Guid, string>();

            foreach (var (guid, info) in _disk.AssetDatabaseInfo.Assets)
            {
                _guidPathDict.Add(guid, info.Path);
            }
        }

        internal async Task<T> GetAssetAsync<T>(string path) where T : AssetResourceBase
        {
            if (_guidPathDict.TryGetByValue(path, out var guid))
            {
                return await GetAssetAsync<T>(guid);
            }

            Debug.Error($"Asset doesn't exists at path: {path}");

            return default;
        }

        internal T GetAsset<T>(string path) where T : AssetResourceBase
        {
            if (_guidPathDict.TryGetByValue(path, out var guid))
            {
                return GetAsset<T>(guid);
            }

            Debug.Error($"Asset doesn't exists at path: {path}");

            return default;
        }

        private async Task<T> GetAssetAsync<T>(Guid guid) where T : AssetResourceBase
        {
            var assetContent = await Disk.GetAssetAsync(guid);

            return BuildAsset(assetContent.Info, guid, assetContent.RawData) as T;
        }

        private T GetAsset<T>(Guid guid) where T : AssetResourceBase
        {
            var assetContent = Disk.GetAssetAsync(guid).GetAwaiter().GetResult();
            var rawAsset = BuildAsset(assetContent.Info, guid, assetContent.RawData);

            if (rawAsset == null)
            {
                throw new Exception($"Fatal: Asset is null: {guid}");
            }

            T asset = rawAsset as T;

            if (asset == null)
            {
                Debug.Error($"Invalid asset cast to type: {typeof(T).Name}, asset type is: {rawAsset.GetType().Name}");
            }

            return asset;
        }

        private AssetResourceBase BuildAsset(AssetInfo info, Guid guid, byte[] rawData)
        {
            var encoding = Encoding.Default;

            if (info.Type == AssetType.Text)
            {
                encoding = Encoding.UTF8;
            }

            using var mem = new MemoryStream(rawData);

            var reader = new BinaryReader(mem, encoding);


            if (_assetbuilder.TryGetValue(info.Type, out AssetBuilderBase builder))
            {
                //if (info.IsEncrypted)
                //{
                //    await Task.Run(() => reader = new BinaryReader(AssetEncrypter.DecryptFromStream(reader.BaseStream, AssetUtils.ENCRYPTION_VERY_SECURE_PASSWORD)));
                //}

                //if (info.IsCompressed)
                //{
                //    await Task.Run(() => reader = new BinaryReader(AssetCompressor.DecompressStream(reader.BaseStream)));
                //}

                if (info.IsEncrypted)
                {
                    reader = new BinaryReader(AssetEncrypter.DecryptFromStream(reader.BaseStream, AssetUtils.ENCRYPTION_VERY_SECURE_PASSWORD));
                }

                if (info.IsCompressed)
                {
                    reader = new BinaryReader(AssetCompressor.DecompressStream(reader.BaseStream));
                }

                var asset = builder.BuildAsset(info, guid, reader);

                reader.Dispose();
                return asset;
            }

            Debug.Error($"Builder for asset type '{info.Type}' was not found");
            return null;
        }
    }
}