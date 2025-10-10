using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal class ReleaseModeDisk : DiskBase
    {
        private readonly BinaryReader _reader;
        private struct AssetLocInfo
        {
            public long AssetDataLoc { get; set; }
            public int AssetDataSize { get; set; }
            public long AssetMetaLoc { get; set; }
            public int AssetMetaSize { get; set; }
        }

        private Dictionary<Guid, AssetLocInfo> _assetsLocations;
        public ReleaseModeDisk(string folderPath)
        {
            var fstream = new FileStream(Path.Combine(folderPath, Paths.GetAssetBuildDataFilename()), FileMode.Open, FileAccess.Read);
            _reader = new BinaryReader(fstream);

            _assetsLocations = new Dictionary<Guid, AssetLocInfo>();
        }

        public override bool Initialize()
        {
            var header = _reader.ReadBytes(AssetUtils.GFSFileFormat.HEADER.Length);

            var headerStr = Encoding.UTF8.GetString(header);

            if (!headerStr.Equals(AssetUtils.GFSFileFormat.HEADER))
            {
                throw new Exception("Corrupted file data");
            }

            var totalAssets = _reader.ReadInt32();
            AssetDatabaseInfo.Assets.EnsureCapacity(totalAssets);
            _assetsLocations.EnsureCapacity(totalAssets);

            var creationTimeBuffer = _reader.ReadInt64();
            AssetDatabaseInfo.CreationDate = DateTime.FromBinary(creationTimeBuffer);
            AssetDatabaseInfo.TotalAssets = totalAssets;

            for (int i = 0; i < totalAssets; i++)
            {
                long assetBlockLoc = _reader.ReadInt64();
                long assetDataLoc = _reader.ReadInt64();
                long metaBlockLoc = _reader.ReadInt64();
                long currentPos = _reader.BaseStream.Position;

                _reader.BaseStream.Position = assetBlockLoc;

                int guidSize = _reader.ReadInt32();
                var guid = new Guid(_reader.ReadBytes(guidSize));

                int pathSize = _reader.ReadInt32();

                string path = Encoding.UTF8.GetString(_reader.ReadBytes(pathSize));

                var assetType = (AssetType)_reader.ReadInt32();
                bool isCompressed = _reader.ReadBoolean();
                bool isEncrypted = _reader.ReadBoolean();
                int assetDataSize = _reader.ReadInt32();
                int metaDataSize = _reader.ReadInt32();

                AssetDatabaseInfo.Assets.Add(guid, new AssetInfo()
                {
                    Type = assetType,
                    IsCompressed = isCompressed,
                    IsEncrypted = isEncrypted,
                    Path = path,
                });

                _assetsLocations.Add(guid, new AssetLocInfo()
                {
                    AssetDataLoc = assetDataLoc,
                    AssetDataSize = assetDataSize,
                    AssetMetaLoc = metaBlockLoc,
                    AssetMetaSize = metaDataSize,
                });

                _reader.BaseStream.Position = currentPos;
            }

            return true;
        }

        protected override byte[] LoadAssetFromDisk(Guid guid)
        {
            if (_assetsLocations.TryGetValue(guid, out var locations))
            {
                _reader.BaseStream.Position = locations.AssetDataLoc;
                return _reader.ReadBytes(locations.AssetDataSize);
            }

            return null;
        }

        protected override async Task<byte[]> LoadAssetFromDiskAsync(Guid guid)
        {
            if (_assetsLocations.TryGetValue(guid, out var locations))
            {
                _reader.BaseStream.Position = locations.AssetDataLoc;
                var data = new byte[locations.AssetDataSize];
                var bytesRead = 0;
                while (bytesRead < data.Length)
                {
                    int read = await _reader.BaseStream.ReadAsync(data, bytesRead, data.Length - bytesRead);
                    if (read == 0)
                    {
                        throw new EndOfStreamException("Unexpected end of stream while reading asset data.");
                    }
                    bytesRead += read;
                }
                return data;
            }

            return null;
        }

        protected override byte[] LoadMetaFromDisk(Guid guid)
        {
            if (_assetsLocations.TryGetValue(guid, out var locations))
            {
                _reader.BaseStream.Position = locations.AssetMetaLoc;
                var meta = new byte[locations.AssetMetaSize];

                _reader.BaseStream.Read(meta, 0, meta.Length);

                return meta;
            }

            return null;
        }
    }
}
