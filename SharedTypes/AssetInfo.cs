using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public class AssetInfo
    {
        public AssetType Type { get; set; } = AssetType.Invalid;
        public DateTime LastWriteTime { get; set; }
        public string Path { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsEncrypted { get; set; }
    }

}
