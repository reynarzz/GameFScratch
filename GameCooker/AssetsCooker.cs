using Newtonsoft.Json;
using SharedTypes;

namespace GameCooker
{
    public class AssetsCooker
    {
        private Dictionary<string, AssetType> _assetsTypes;
        private Dictionary<AssetType, IAssetProcessor> _assetsProcessors;
        private Dictionary<CookingType, AssetsCookerBase> _assetCookers;

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

            _assetCookers = new Dictionary<CookingType, AssetsCookerBase>()
            {
                {  CookingType.DevMode, new DevModeFilesCooker() },
                {  CookingType.ReleaseMode, new ReleaseModeFilesCooker() },
            };

            if (File.Exists(Paths.GetAssetDatabaseFilePath()))
            {
                _databaseInfo = JsonConvert.DeserializeObject<AssetsDatabaseInfo>(File.ReadAllText(Paths.GetAssetDatabaseFilePath()));
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

        public async Task<AssetsDatabaseInfo> CookAllAsync(CookOptions options, string assetsRootFolder, string folderOut)
        {
            var files = Directory.GetFiles(assetsRootFolder, "*", SearchOption.AllDirectories).Where(x => !x.EndsWith(Paths.ASSET_META_EXT_NAME));

            var selectedFiles = files.Where(path => _assetsTypes.TryGetValue(Path.GetExtension(path), out _))
                                     .Select(path => (path.Replace("\\", "/"), _assetsTypes[Path.GetExtension(path)]));

            await _assetCookers[options.Type].CookAssetsAsync(selectedFiles, ProcessAsset, _databaseInfo, folderOut);

            return _databaseInfo;
        }

        public AssetsDatabaseInfo CookAll(CookOptions options, string assetsRootFolder, string folderOut)
        {
            return CookAllAsync(options, assetsRootFolder, folderOut).Result;
        }

        private byte[] ProcessAsset(AssetType type, string path)
        {
            if (_assetsProcessors.TryGetValue(type, out var processor))
            {
                return processor.Process(path);
            }

            return [];
        }
    }
}