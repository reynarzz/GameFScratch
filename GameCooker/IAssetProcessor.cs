using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    internal interface IAssetProcessor
    {
        byte[] Process(string path, AssetMetaFileBase meta);
    }
}
