using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal class TextAssetBuilder : AssetBuilderBase
    {
        internal override AssetResourceBase BuildAsset(AssetInfo info, AssetMetaFileBase meta, Guid guid, BinaryReader reader)
        {
            var length = reader.BaseStream.Length;
            var data = new byte[length];
            int bytesRead =  reader.BaseStream.Read(data, 0, (int)length);
            string text = Encoding.UTF8.GetString(data, 0, bytesRead);

            return new TextAsset(text, info.Path, guid);
        }
    }
}
