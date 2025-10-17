using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal abstract class AssetBuilderBase
    {
        internal abstract AssetResourceBase BuildAsset(AssetInfo info, Guid guid, BinaryReader reader);
    }
}