using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    [Serializable]
    internal struct TextureConfig
    {
        public bool IsNearest { get; set; }
        public bool IsAtlas { get; set; }
    }

    [Serializable]
    internal class TextureMetaFile : AssetMetaFileBase
    {
        public TextureConfig Config { get; set; }
    }
}