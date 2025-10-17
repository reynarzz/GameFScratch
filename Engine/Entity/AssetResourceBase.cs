using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class AssetResourceBase : EObject
    {
        public string Path { get; }
        public AssetResourceBase(string path, Guid guid) : base(System.IO.Path.GetFileNameWithoutExtension(path), guid)
        {
            Path = path;
        }
    }
}
