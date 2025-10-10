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
        //internal override async Task<EObject> BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        //{
        //    var width = reader.ReadInt32();
        //    var height = reader.ReadInt32();
        //    var comp = reader.ReadInt32();

        //    var imageData = reader.ReadBytes(width * height * comp);

        //    return new Texture2D(width, height, comp, imageData);
        //}

        internal override EObject BuildAsset(AssetInfo info, Guid guid, BinaryReader reader)
        {
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var comp = reader.ReadInt32();

            int imageSize = checked(width * height * comp);
            var imageData = new byte[imageSize];

            int bytesRead = 0;
            while (bytesRead < imageSize)
            {
                int read = reader.BaseStream.Read(imageData, bytesRead, imageSize - bytesRead);

                if (read == 0)
                {
                    throw new EndOfStreamException("Unexpected end of stream while reading texture data.");
                }
                bytesRead += read;
            }

            return new Texture2D(width, height, comp, imageData);
        }
    }
}