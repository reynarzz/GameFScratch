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
