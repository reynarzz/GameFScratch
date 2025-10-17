using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    [Serializable]
    public struct TextureConfig
    {
        public bool IsNearest { get; set; }
        public bool IsAtlas { get; set; }
        public int Mode { get; set; }
    }

    [Serializable]
    public class TextureMetaFile : AssetMetaFileBase
    {
        public TextureConfig Config { get; set; }
    }
}