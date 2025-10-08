using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace GameCooker
{
    public enum CookingType
    {
        Monolith,
        SeparatedFiles
    }

    public struct CookOptions
    {
        public CookingType Type { get; set; }
    }

    public enum AssetType
    {
        Invalid,
        Texture,
        Audio,
        Text
    }

    internal interface IAssetProcessor
    {
        byte[] Process(string path);
    }

    public class AssetsDatabaseInfo
    {
        public int TotalAssets { get; set; }
        public DateTime CreationDate { get; set; }
        public Dictionary<Guid, AssetInfo> Assets { get; private set; } = new();
    }

    public class AssetInfo
    {
        public AssetType Type { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string Path { get; set; }
    }

    public class AssetsCooker
    {
        private Dictionary<string, AssetType> _assetsTypes;
        private Dictionary<AssetType, IAssetProcessor> _assetsProcessors;
        private AssetsDatabaseInfo _databaseInfo;

        public AssetsCooker()
        {
            _assetsTypes = new Dictionary<string, AssetType>(StringComparer.OrdinalIgnoreCase)
            {
                // Image
                { ".png", AssetType.Texture },
                { ".tga", AssetType.Texture },
                { ".jpg", AssetType.Texture },

                // Audio
                { ".mp3", AssetType.Audio },
                { ".wav", AssetType.Audio },

                // Text
                { ".txt", AssetType.Text },
                { ".ldtk", AssetType.Text },
                { ".json", AssetType.Text },
                { ".xml", AssetType.Text },
                { ".shader", AssetType.Text },
                { ".vert", AssetType.Text },
                { ".vertex", AssetType.Text },
                { ".frag", AssetType.Text },
                { ".fragment", AssetType.Text },
            };

            _assetsProcessors = new Dictionary<AssetType, IAssetProcessor>()
            {
                { AssetType.Texture, new TextureAssetProcessor() },
                { AssetType.Text, new TextAssetProcessor() }
            };

            if (File.Exists(ProjectPaths.GetAssetDatabaseFilePath()))
            {
                _databaseInfo = JsonConvert.DeserializeObject<AssetsDatabaseInfo>(File.ReadAllText(ProjectPaths.GetAssetDatabaseFilePath()));
            }

            if (_databaseInfo == null)
            {
                _databaseInfo = new AssetsDatabaseInfo()
                {
                    CreationDate = DateTime.Now,
                    TotalAssets = 0
                };
            }
        }

        public async Task<AssetsDatabaseInfo> CookAll(CookOptions options, string assetsRootFolder, string folderOut)
        {
            var files = Directory.GetFiles(assetsRootFolder, ".", SearchOption.AllDirectories).Where(x => !x.EndsWith(ProjectPaths.ASSET_META_EXT_NAME));

            foreach (var filepath in files)
            {
                var fileCleanPath = filepath.Replace("\\", "/");

                // Console.WriteLine(file + ", etx: " + Path.GetExtension(file));
                if (!_assetsTypes.TryGetValue(Path.GetExtension(fileCleanPath), out var assetType))
                    continue;

                var meta = GetMeta(fileCleanPath, assetType);

                AssetInfo assetInfo = null;

                bool constainsAssetInfo = meta != null ? _databaseInfo.Assets.TryGetValue(meta.GUID, out assetInfo) : false;

                bool isInLibrary = false;

                var binPath = ProjectPaths.CreateAssetDatabaseBinFilePath(meta.GUID.ToString());

                if (meta != null && File.Exists(binPath))
                {
                    isInLibrary = true;
                }

                var latestWriteTime = File.GetLastWriteTime(fileCleanPath);

                if (!constainsAssetInfo || latestWriteTime > assetInfo.LastWriteTime || !isInLibrary)
                {
                    var data = ProcessAsset(fileCleanPath);

                    var assetRelPath = ProjectPaths.GetRelativeAssetPath(fileCleanPath);
                    if (data != null)
                    {
                        if (constainsAssetInfo)
                        {
                            Console.WriteLine("Updating asset file: " + fileCleanPath);
                            assetInfo = _databaseInfo.Assets[meta.GUID];

                            assetInfo.LastWriteTime = latestWriteTime;
                            assetInfo.Path = assetRelPath;
                        }
                        else
                        {
                            Console.WriteLine("Importing asset file: " + fileCleanPath);
                            var guid = meta != null ? meta.GUID : Guid.NewGuid();
                            _databaseInfo.Assets.Add(guid, new AssetInfo() { LastWriteTime = latestWriteTime, Type = assetType, Path = assetRelPath });
                            // Write meta
                            File.WriteAllText(fileCleanPath + ProjectPaths.ASSET_META_EXT_NAME, JsonConvert.SerializeObject(meta, Formatting.Indented));
                        }

                        // Write asset to library
                        File.WriteAllBytes(folderOut + "/" + meta.GUID + ".bin", data);
                    }
                }
            }
            
            _databaseInfo.TotalAssets = _databaseInfo.Assets.Count;

            // Write asset database
            File.WriteAllText(ProjectPaths.GetAssetDatabaseFilePath(), JsonConvert.SerializeObject(_databaseInfo, Formatting.Indented));

            return _databaseInfo;
        }

        internal class AssetData
        {
            internal AssetType Type { get; set; }
            internal byte[] Data { get; set; }
            internal AssetMetaFileBase Meta { get; set; }
        }

        private byte[] ProcessAsset(string path)
        {
            if (_assetsTypes.TryGetValue(Path.GetExtension(path), out var assetType))
            {
                if (_assetsProcessors.TryGetValue(assetType, out var processor))
                {
                    return processor.Process(path);
                }
            }

            return null;
        }

        private AssetMetaFileBase GetMeta(string path, AssetType assetType)
        {
            var metaFilePath = path + ProjectPaths.ASSET_META_EXT_NAME;
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
