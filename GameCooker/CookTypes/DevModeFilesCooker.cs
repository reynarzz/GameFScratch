using SharedTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    internal class DevModeFilesCooker : AssetsCookerBase
    {
        private AssetsDatabaseInfo _database;

        public DevModeFilesCooker(AssetsDatabaseInfo database)
        {
            _database = database;
        }

        internal override async Task CookAssetsAsync(CookFileOptions fileOptions, (string, AssetType)[] files, Func<AssetType, string, byte[]> processAssetCallback,
                                                     string outFolder)
        {
            foreach (var (filePath, assetType) in files)
            {
                var meta = AssetUtils.GetMeta(filePath + Paths.ASSET_META_EXT_NAME, assetType);

                AssetInfo assetInfo = null;

                bool constainsAssetInfo = meta != null ? _database.Assets.TryGetValue(meta.GUID, out assetInfo) : false;

                bool isInLibrary = false;

                var binPath = Paths.CreateBinFilePath(outFolder, meta.GUID.ToString());

                if (meta != null && File.Exists(binPath))
                {
                    isInLibrary = true;
                }

                var latestWriteTime = File.GetLastWriteTime(filePath);

                if (!constainsAssetInfo || latestWriteTime > assetInfo.LastWriteTime || !isInLibrary)
                {
                    var data = processAssetCallback(assetType, filePath);

                    var assetRelPath = Paths.GetRelativeAssetPath(filePath);
                    if (data != null)
                    {
                        if (constainsAssetInfo)
                        {
                            Console.WriteLine("Updating asset file: " + filePath);
                            assetInfo = _database.Assets[meta.GUID];

                            assetInfo.LastWriteTime = latestWriteTime;
                            assetInfo.Path = assetRelPath;
                        }
                        else
                        {
                            Console.WriteLine("Importing asset file: " + filePath);
                            var guid = meta != null ? meta.GUID : Guid.NewGuid();
                            _database.Assets.Add(guid, new AssetInfo() { LastWriteTime = latestWriteTime, Type = assetType, Path = assetRelPath, IsEncrypted = false, IsCompressed = false });
                            // Write meta
                            File.WriteAllText(filePath + Paths.ASSET_META_EXT_NAME, JsonConvert.SerializeObject(meta, Formatting.Indented));
                        }

                        // Write asset to library
                        File.WriteAllBytes(binPath, data);
                    }
                }
            }

            _database.TotalAssets = _database.Assets.Count;

            // Write asset database
            File.WriteAllText(Path.Combine(outFolder, Paths.ASSET_DATABASE_FILE_NAME), JsonConvert.SerializeObject(_database, Formatting.Indented));
        }
    }
}