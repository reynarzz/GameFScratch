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
        internal override EObject BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes((int)reader.BaseStream.Length);

            string text = Encoding.UTF8.GetString(bytes);

            return new TextAsset(text, info.Path, guid);
        }
    }
}
