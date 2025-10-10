using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Engine.Layers;

namespace Engine
{
    public class Assets
    {
        public static async Task<TextAsset> GetTextAsync(string path)
        {
            return await IOLayer.GetDatabase().GetAssetAsync<TextAsset>(path);
        }

        public static async Task<Texture2D> GetTextureAsync(string path)
        {
            return await IOLayer.GetDatabase().GetAssetAsync<Texture2D>(path);
        }

        public static TextAsset GetText(string path)
        {
            return IOLayer.GetDatabase().GetAsset<TextAsset>(path);
        }

        public static Texture2D GetTexture(string path)
        {
            return IOLayer.GetDatabase().GetAsset<Texture2D>(path);
        }
    }
}
