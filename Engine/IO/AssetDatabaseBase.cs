using Engine.Utils;
using GameCooker;// remove
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal abstract class AssetDatabaseBase
    {
        private readonly Dictionary<AssetType, AssetBuilderBase> _assetbuilder;
        private BiDictionary<Guid, string> _guidPathDict;
        private AssetsDatabaseInfo _database;

        protected AssetDatabaseBase()
        {
            _assetbuilder = new()
            {
                { AssetType.Texture, new TextureAssetBuilder() },
                { AssetType.Text, new TextAssetBuilder() }
            };
        }

        internal virtual void Initialize(AssetsDatabaseInfo database)
        {
            _database = database;
            _guidPathDict = new BiDictionary<Guid, string>();

            foreach (var asset in database.Assets)
            {
                _guidPathDict.Add(asset.Key, asset.Value.Path);
            }
        }

        internal T GetAsset<T>(string path) where T : EObject
        {
            if (_guidPathDict.TryGetByValue(path, out var guid))
            {
                return GetAsset<T>(guid, _database.Assets[guid]);
            }

            Debug.Error($"Asset doesn't exists at path: {path}");

            return default;
        }

        protected abstract T GetAsset<T>(Guid guid, AssetInfo info) where T : EObject;

        protected EObject BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            if (_assetbuilder.TryGetValue(info.Type, out AssetBuilderBase builder))
            {
                if (info.IsEncrypted)
                {
                    reader = new BinaryReader(AssetEncrypter.DecryptFromStream(reader.BaseStream, "1234"));
                }

                if (info.IsCompressed)
                {
                    reader = new BinaryReader(AssetCompressor.DecompressStream(reader.BaseStream));
                }

                return builder.BuildAsset(info, guid, reader);
            }

            Debug.Error($"Builder for asset type '{info.Type}' was not found");
            return null;
        }
    }
}