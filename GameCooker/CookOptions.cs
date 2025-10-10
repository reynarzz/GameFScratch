using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    public struct CookFileOptions
    {
        public bool CompressAllFiles { get; set; }
        public bool EncryptAllFiles { get; set; }

        public HashSet<AssetType> FilesToCompress { get; private set; } = new();
        public HashSet<AssetType> FilesToEncrypt { get; private set; } = new();
        public bool EncryptFilesPath { get; set; }

        public CookFileOptions() { }
    }

    public struct CookOptions
    {
        public CookingType Type { get; set; }
        public string AssetsFolderPath { get; set; }
        public string ExportFolderPath { get; set; }
        public CookFileOptions FileOptions { get; set; }
    }
}
