using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AssetVersionAttribute : Attribute
    {
        public int Version { get; }
        public AssetVersionAttribute(int version) => Version = version;
    }
}
