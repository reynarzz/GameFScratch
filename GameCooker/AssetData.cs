using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    internal class AssetData
    {
        internal AssetType Type { get; set; }
        internal byte[] Data { get; set; }
        internal AssetMetaFileBase Meta { get; set; }
    }
}
