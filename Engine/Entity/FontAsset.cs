using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class FontAsset : AssetResourceBase
    {
        internal byte[] Data { get; }

        public FontAsset(string path, Guid guid, byte[] data) : base(path, guid)
        {
            Data = data;
        }
    }
}
