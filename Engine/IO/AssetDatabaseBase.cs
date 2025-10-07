using Engine.Utils;
using GameCooker;
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
        private Dictionary<AssetType, AssetBuilderBase> _assetbuilder;
        protected BiDictionary<Guid, string> GuidPathDict { get; private set; }
        protected AssetsDatabaseInfo Database { get; private set; }

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
            Database = database;
            GuidPathDict = new BiDictionary<Guid, string>();

            foreach (var asset in database.Assets)
            {
                GuidPathDict.Add(asset.Key, asset.Value.Path);
            }
        }

        internal abstract T GetAsset<T>(string path) where T : EObject;

        protected EObject BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            if (_assetbuilder.TryGetValue(info.Type, out AssetBuilderBase builder))
            {
                return builder.BuildAsset(info, guid, reader);
            }

            Debug.Error($"Builder for asset type '{info.Type}' was not found");
            return null;
        }
    }
}