using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public static class AssetUtils
    {
        public const string ENCRYPTION_VERY_SECURE_PASSWORD = "ThisDefinitelyShouldNotBeHere";
        public static class GFSFileFormat
        {
            public const string HEADER = "GFSD";
        }

        public static AssetMetaFileBase GetMeta(string path, AssetType assetType)
        {
            string metaJson = null;

            if (File.Exists(path))
            {
                metaJson = File.ReadAllText(path);
            }

            AssetMetaFileBase GetMeta<T>(string json) where T : AssetMetaFileBase, new()
            {
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }

                return new T { GUID = Guid.NewGuid() };
            }

            return assetType switch
            {
                AssetType.Invalid => GetMeta<DefaultMetaFile>(metaJson),
                AssetType.Texture => GetMeta<TextureMetaFile>(metaJson),
                AssetType.Audio => GetMeta<AudioMetaFile>(metaJson),
                AssetType.Text => GetMeta<DefaultMetaFile>(metaJson),
                AssetType.Shader => GetMeta<DefaultMetaFile>(metaJson),
                _ => GetMeta<DefaultMetaFile>(metaJson)
            };
        }
    }
}
