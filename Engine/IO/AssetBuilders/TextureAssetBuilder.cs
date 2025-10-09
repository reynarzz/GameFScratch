using SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.IO
{
    internal class TextureAssetBuilder : AssetBuilderBase
    {
        internal override EObject BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var comp = reader.ReadInt32();

            var imageData = reader.ReadBytes(width * height * comp);

            return new Texture2D(width, height, comp, imageData);
        }
    }
}