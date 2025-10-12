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
                { ".psd", AssetType.Texture },
                { ".hdr", AssetType.Texture },
                { ".pic", AssetType.Texture },
                { ".bmp", AssetType.Texture },

                // Audio
                { ".mp3", AssetType.Audio },
                { ".wav", AssetType.Audio },
                { ".aac", AssetType.Audio },
                { ".wma", AssetType.Audio },
                { ".flac", AssetType.Audio },
                

                // Text
                { ".txt", AssetType.Text },
                { ".ldtk", AssetType.Text },
                { ".json", AssetType.Text },
                { ".xml", AssetType.Text },

                // Shader
                { ".shader", AssetType.Shader },
                { ".glsl", AssetType.Shader },
                { ".vert", AssetType.Shader },
                { ".vertex", AssetType.Shader },
                { ".frag", AssetType.Shader },
                { ".fragment", AssetType.Shader },
            };

            _assetsProcessors = new Dictionary<AssetType, IAssetProcessor>()
            {
                { AssetType.Texture, new TextureAssetProcessor() },
                { AssetType.Audio, new AudioAssetProcessor() },
                { AssetType.Text, new TextAssetProcessor() },
                { AssetType.Shader, new TextAssetProcessor() }

            };

            if (File.Exists(Paths.GetAssetDatabaseFilePath()))
            {
                _databaseInfo = JsonConvert.DeserializeObject<AssetsDatabaseInfo>(File.ReadAllText(Paths.GetAssetDatabaseFilePath()));
            }

            if (_databaseInfo == null)
            {
                _databaseInfo = new AssetsDatabaseInfo()
                {
                    CreationDate = DateTime.Now
                };
            }
            
            _assetCookers = new Dictionary<CookingType, AssetsCookerBase>()
            {
                {  CookingType.DevMode, new DevModeFilesCooker(_databaseInfo) },
                {  CookingType.ReleaseMode, new ReleaseModeFilesCooker() },
            };
        }

        public async Task<AssetsDatabaseInfo> CookAllAsync(CookOptions options)
        {
            var files = Directory.GetFiles(options.AssetsFolderPath, "*", SearchOption.AllDirectories).Where(x => !x.EndsWith(Paths.ASSET_META_EXT_NAME));

            var selectedFiles = files.Where(path => _assetsTypes.TryGetValue(Path.GetExtension(path), out _))
                                     .Select(path => (path.Replace("\\", "/"), _assetsTypes[Path.GetExtension(path)]));

            await _assetCookers[options.Type].CookAssetsAsync(options.FileOptions, selectedFiles.ToArray(), ProcessAsset, options.ExportFolderPath);

            return _databaseInfo;
        }

        public AssetsDatabaseInfo CookAll(CookOptions options)
        {
            return CookAllAsync(options).GetAwaiter().GetResult();
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