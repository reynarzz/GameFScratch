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

        internal override async Task CookAssetsAsync(CookFileOptions fileOptions, (string, AssetType)[] files,
                                                     Func<AssetType, AssetMetaFileBase, string, byte[]> processAssetCallback,
                                                     string outFolder)
        {
            foreach (var (filePath, assetType) in files)
            {
                var metaPath = filePath + Paths.ASSET_META_EXT_NAME;

                var meta = AssetUtils.GetMeta(metaPath, assetType);

                AssetInfo assetInfo = null;

                bool constainsAssetInfo = _database.Assets.TryGetValue(meta.GUID, out assetInfo);

                bool isInLibrary = false;

                var binPath = Paths.CreateBinFilePath(outFolder, meta.GUID.ToString());

                if (meta != null && File.Exists(binPath))
                {
                    isInLibrary = true;
                }

                var latestWriteTime = File.GetLastWriteTime(filePath);
                var metaLatestWriteTime = File.GetLastWriteTime(metaPath);

                if (!constainsAssetInfo || latestWriteTime > assetInfo.LastWriteTime ||
                    !isInLibrary || metaLatestWriteTime > assetInfo.MetaWriteTime)
                {
                    var data = processAssetCallback(assetType, meta, filePath);

                    var assetRelPath = Paths.GetRelativeAssetPath(filePath);
                    if (data != null)
                    {
                        if (constainsAssetInfo)
                        {
                            Console.WriteLine("Updating asset file: " + filePath);
                            assetInfo = _database.Assets[meta.GUID];

                            assetInfo.LastWriteTime = latestWriteTime;
                            assetInfo.MetaWriteTime = metaLatestWriteTime;
                            assetInfo.Path = assetRelPath;
                        }
                        else
                        {
                            Console.WriteLine("Importing asset file: " + filePath);

                            // Write meta
                            File.WriteAllText(metaPath, JsonConvert.SerializeObject(meta, Formatting.Indented));

                            _database.Assets.Add(meta.GUID, new AssetInfo()
                            {
                                Type = assetType,
                                Path = assetRelPath,
                                IsEncrypted = false,
                                IsCompressed = false,
                                LastWriteTime = latestWriteTime,
                                MetaWriteTime = File.GetLastWriteTime(metaPath)
                            });
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