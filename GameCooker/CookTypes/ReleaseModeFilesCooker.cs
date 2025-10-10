using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    internal class ReleaseModeFilesCooker : AssetsCookerBase
    {
        /* .gfs file format
         
            - Magic (char[4])
            
            Data info    
            [
                - total assets (int32)
                - creation date (int64)
            ]
            
            Location table
            [
                - asset block location (int64)
                - asset data (in block) location (int64)
                - asset's meta (in block) location (int64)
            }
            
            Assets block (n times)
            [
                - asset guid size (int32)
                - asset guid (byte[])
                - asset path size (int32)
                - asset path (byte[])
                - asset type (int32)
                - isCompressed (bool)
                - isEncrypted (bool)
                - asset data size (int32)
                - meta data size (int32)
                - asset data (byte[])
                - meta data (format defined by asset type)
            ]
         */

        private long metaLocSize = sizeof(long);
        private long assetBlockLocSize = sizeof(long);
        private long assetDataLocSize = sizeof(long);
        private long fieldIfOffset => assetBlockLocSize + assetDataLocSize + metaLocSize;

        private const int TEMP_BUFFER_SIZE = 81920;
       
        internal override async Task CookAssetsAsync((string, AssetType)[] files,
                                                    Func<AssetType, string, byte[]> processAssetCallback,
                                                    string outFolder)
        {
            var path = Path.Combine(outFolder, Paths.GetAssetBuildDataFilename());
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, TEMP_BUFFER_SIZE, useAsync: true);
            using var bufWritter = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);

            // Writes header
            bufWritter.Write(Encoding.ASCII.GetBytes(AssetUtils.GFSFileFormat.HEADER));

            // Writes total asset files
            bufWritter.Write(files.Length);

            // creation date
            bufWritter.Write(DateTime.Now.ToBinary());

            var currentFileIdPosition = bufWritter.BaseStream.Position;

            long currentAssetOffset = fieldIfOffset * files.Length;

            bufWritter.BaseStream.Position += currentAssetOffset;

            bool isEncryptedFile = false;
            bool isCompressedFile = false;

            foreach (var (filePath, assetType) in files)
            {
                long startAssetBlockPos = bufWritter.BaseStream.Position;

                var meta = AssetUtils.GetMeta(filePath + Paths.ASSET_META_EXT_NAME, assetType);
                var relAssetPath = Paths.GetRelativeAssetPath(filePath);

                var guidBinary = meta.GUID.ToByteArray();
                // asset guid size
                bufWritter.Write(guidBinary.Length);

                // asset guid
                bufWritter.Write(guidBinary);

                // asset path size
                bufWritter.Write(relAssetPath.Length);

                // asset path
                bufWritter.Write(Encoding.UTF8.GetBytes(relAssetPath));

                // Writes asset type
                bufWritter.Write((int)assetType);

                // is compressed
                bufWritter.Write(isCompressedFile);

                // is encrypted
                bufWritter.Write(isEncryptedFile);

                var assetData = await Task.Run(() => processAssetCallback(assetType, filePath));

                if (isCompressedFile)
                {
                    assetData = await Task.Run(() => AssetCompressor.CompressBytes(assetData));
                }

                if (isEncryptedFile)
                {
                    assetData = await Task.Run(() => AssetEncrypter.EncryptBytes(assetData, AssetUtils.ENCRYPTION_VERY_SECURE_PASSWORD));
                }

                // Asset length
                bufWritter.Write(assetData.Length);

                var metaBuffer = MetadataBufferWriter(bufWritter, meta);

                // meta size
                bufWritter.Write(metaBuffer.Length);

                long assetDataLoc = bufWritter.BaseStream.Position;
                // Asset data
                bufWritter.Write(assetData);


                long metaStartLocation = bufWritter.BaseStream.Position;

                // metadata
                bufWritter.Write(metaBuffer);

                long assetEndPos = bufWritter.BaseStream.Position;

                // ---- Set table position to write asset info
                bufWritter.BaseStream.Position = currentFileIdPosition;

                
                // asset block location
                bufWritter.Write(startAssetBlockPos);

                // asset data location
                bufWritter.Write(assetDataLoc);

                // asset's meta location
                bufWritter.Write(metaStartLocation);

                // apply current asset block write position
                bufWritter.BaseStream.Position = assetEndPos;

                // advance file info position in table
                currentFileIdPosition += fieldIfOffset;
            }

            bufWritter.Flush();
            await fs.FlushAsync();
        }

        private byte[] MetadataBufferWriter(BinaryWriter writer, AssetMetaFileBase meta)
        {
            return [8, 2, 6, 4, 9, 1, 7, 0, 3, 5];
        }
    }
}