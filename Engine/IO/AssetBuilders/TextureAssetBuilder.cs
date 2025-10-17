using Engine.Layers;
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
        internal override AssetResourceBase BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var comp = reader.ReadInt32();

            int imageSize = checked(width * height * comp);
            var imageData = new byte[imageSize];

            reader.BaseStream.ReadExactly(imageData);

            return new Texture2D(info.Path, guid, width, height, comp, imageData);
        }
    }
}