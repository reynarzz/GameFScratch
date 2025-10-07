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

    public class AssetsCooker
    {
        private Dictionary<string, AssetType> _assetsTypes;
        private Dictionary<AssetType, IAssetProcessor> _assetsProcessors;
        private AssetsDatabaseInfo _databaseInfo;

        public class AssetInfo
        {
            public AssetType Type { get; set; }
            public DateTime LastWriteTime { get; set; }
        }


        public class AssetsDatabaseInfo
        {
            public DateTime CreationDate { get; set; }
            public Dictionary<Guid, AssetInfo> Assets { get; private set; } = new();
        }
        private const string AssetDatabaseFileTestPath = "D:\\Projects\\GameScratch\\Game\\Library\\AssetsDatabase\\" + ASSET_DATABASE_FILE_NAME;


        private const string ASSET_DATABASE_FILE_NAME = "AssetDatabase.txt";
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

            if (File.Exists(AssetDatabaseFileTestPath))
            {
                _databaseInfo = JsonConvert.DeserializeObject<AssetsDatabaseInfo>(File.ReadAllText(AssetDatabaseFileTestPath));
            }

            if (_databaseInfo == null)
            {
                _databaseInfo = new AssetsDatabaseInfo()
                {
                    CreationDate = DateTime.Now,
                };
            }
        }

        public async Task<bool> CookAll(CookOptions options, string assetsRootFolder, string folderOut)
        {
            var files = Directory.GetFiles(assetsRootFolder, ".", SearchOption.AllDirectories).Where(x => !x.EndsWith(".meta"));

            foreach (var file in files)
            {
                // Console.WriteLine(file + ", etx: " + Path.GetExtension(file));
                if (!_assetsTypes.TryGetValue(Path.GetExtension(file), out var assetType))
                    continue;

                    var meta = GetMeta(file, assetType);

                AssetInfo assetInfo = null;

                var constainsAssetInfo = meta != null ? _databaseInfo.Assets.TryGetValue(meta.GUID, out assetInfo) : false;

                var latestWriteTime = File.GetLastWriteTime(file);

                if (!constainsAssetInfo || latestWriteTime > assetInfo.LastWriteTime)
                {
                    var data = ProcessAsset(file);

                    if (data != null)
                    {
                        if (constainsAssetInfo)
                        {
                            Console.WriteLine("Updating asset file: " + file);
                            _databaseInfo.Assets[meta.GUID].LastWriteTime = latestWriteTime;
                        }
                        else
                        {
                            Console.WriteLine("Importing asset file: " + file);
                            var guid = meta != null ? meta.GUID : Guid.NewGuid();
                            _databaseInfo.Assets.Add(guid, new AssetInfo() { LastWriteTime = latestWriteTime, Type = assetType });
                            // Write meta
                            File.WriteAllText(file + ".meta", JsonConvert.SerializeObject(meta, Formatting.Indented));
                        }

                        // Write asset to library
                        File.WriteAllBytes(folderOut + "\\" + meta.GUID + ".bin", data);
                    }
                }
            }

            // Write asset database
            File.WriteAllText(AssetDatabaseFileTestPath, JsonConvert.SerializeObject(_databaseInfo, Formatting.Indented));

            return true;
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
                    return processor.Process(path); // TODO: don't process if last write time is same in database and real
                }
            }

            return default;
        }

        private AssetMetaFileBase GetMeta(string path, AssetType assetType)
        {
            var metaFilePath = path + ".meta";
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
