using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using StbImageSharp;

namespace Engine
{
    public class Assets
    {
        public static Texture2D GetTexture(string path)
        {
            // TODO: load asset from package, not directly from os.

            StbImage.stbi_set_flip_vertically_on_load(1);

            var result = ImageResult.FromMemory(File.ReadAllBytes(path));

            var texture = new Texture2D(result.Width, result.Height, (int)result.Comp, result.Data);
            texture.Name = Path.GetFileName(path);
            return texture;
        }
    }
}
