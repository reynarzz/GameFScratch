using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    internal class ReleaseModeFilesCooker : AssetsCookerBase
    {
        /*
         .gfs file format
            - Magic (char[4])
            - asset guid length (int)
            - asset guid (string)
            - asset location (long)
            
         */

        public static class ReleaseFormat
        {
            public static string HEADER = "GFSD";


        }

        internal override async Task CookAssetsAsync(IEnumerable<(string, AssetType)> files,
                                                     Func<AssetType, string, byte[]> processAssetCallback,
                                                     AssetsDatabaseInfo database,
                                                     string outFolder)
        {
            using var fileMemStream = new MemoryStream();
            using var ms = new MemoryStream();
            using var bufWritter = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);
            using var fileConfigWritter = new BinaryWriter(fileMemStream, Encoding.UTF8, leaveOpen: true);

            bufWritter.Write(Encoding.ASCII.GetBytes(ReleaseFormat.HEADER));

            foreach (var file in files)
            {
                var filePath = file.Item1;
                var assetType = file.Item2;

                var assetData = processAssetCallback(assetType, filePath);
                var meta = AssetUtils.GetMeta(filePath + Paths.ASSET_META_EXT_NAME, assetType);

                var metaBuffer = MetadataBufferWriter(fileConfigWritter, meta);

                // assetData = AssetEncrypter.EncryptBytes(AssetCompressor.CompressBytes(assetData), "1234");

                bufWritter.Write(assetData);
            }

            File.WriteAllBytes(Path.Combine(outFolder, Paths.GetAssetBuildDataFilename()), ms.ToArray());
        }

        private byte[] MetadataBufferWriter(BinaryWriter writer, AssetMetaFileBase meta)
        {
            writer.Seek(0, SeekOrigin.Begin);

            return null;
        }
    }
}