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
        public const string ENCRYPTION_VERY_SECURE_PASSWORD = "ThisDefinitelyShouldNotBeHereInProdCode";

        public static AssetMetaFileBase GetMeta(string path, AssetType assetType)
        {
            var metaFilePath = path;
            string metaJson = null;

            if (File.Exists(metaFilePath))
            {
                metaJson = File.ReadAllText(metaFilePath);
            }

            AssetMetaFileBase metaFile = null;

            if (string.IsNullOrEmpty(metaJson))
            {
                if (assetType == AssetType.Texture)
                {
                    metaFile = new TextureMetaFile();
                }
                else
                {
                    metaFile = new DefaultMetaFile();
                }

                metaFile.GUID = Guid.NewGuid();
            }
            else
            {
                if (assetType == AssetType.Texture)
                {
                    metaFile = JsonConvert.DeserializeObject<TextureMetaFile>(metaJson);
                }
                else
                {
                    metaFile = JsonConvert.DeserializeObject<DefaultMetaFile>(metaJson);
                }
            }

            return metaFile;
        }
    }
}
