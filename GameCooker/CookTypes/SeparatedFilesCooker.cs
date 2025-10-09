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
    internal class SeparatedFilesCooker : AssetsCookerBase
    {
        internal override async Task CookAssetsAsync(IEnumerable<(string, AssetType)> files, Func<AssetType, string, byte[]> processAssetCallback, 
                                                     AssetsDatabaseInfo database, string outFolder)
        {
            foreach (var file in files.OrderBy(x => x.Item2))
            {
                var filePath = file.Item1;
                var assetType = file.Item2;

                var meta = AssetUtils.GetMeta(filePath + Paths.ASSET_META_EXT_NAME, file.Item2);

                AssetInfo assetInfo = null;

                bool constainsAssetInfo = meta != null ? database.Assets.TryGetValue(meta.GUID, out assetInfo) : false;

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
                            assetInfo = database.Assets[meta.GUID];

                            assetInfo.LastWriteTime = latestWriteTime;
                            assetInfo.Path = assetRelPath;
                        }
                        else
                        {
                            Console.WriteLine("Importing asset file: " + filePath);
                            var guid = meta != null ? meta.GUID : Guid.NewGuid();
                            database.Assets.Add(guid, new AssetInfo() { LastWriteTime = latestWriteTime, Type = assetType, Path = assetRelPath, IsEncrypted = false, IsCompressed = false });
                            // Write meta
                            File.WriteAllText(filePath + Paths.ASSET_META_EXT_NAME, JsonConvert.SerializeObject(meta, Formatting.Indented));
                        }

                        // Write asset to library
                        File.WriteAllBytes(binPath, data);
                    }
                }
            }

            database.TotalAssets = database.Assets.Count;

            // Write asset database
            File.WriteAllText(Path.Combine(outFolder, Paths.ASSET_DATABASE_FILE_NAME), JsonConvert.SerializeObject(database, Formatting.Indented));
        }
    }
}