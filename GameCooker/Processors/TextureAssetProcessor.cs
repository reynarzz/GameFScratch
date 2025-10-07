using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureBinary
    {
        public int Width;
        public int Height;
        public int Comp;
        public byte[] Bytes;
    }

    internal class TextureAssetProcessor : IAssetProcessor
    {
        byte[] IAssetProcessor.Process(string path)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);

            var result = ImageResult.FromMemory(File.ReadAllBytes(path));

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(result.Width);
                    bw.Write(result.Height);
                    bw.Write((int)result.Comp);
                    bw.Write(result.Data);
                    bw.Flush();

                    return ms.ToArray();
                }
            }
        }
    }
}