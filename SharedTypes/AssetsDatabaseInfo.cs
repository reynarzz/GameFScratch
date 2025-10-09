using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedTypes
{
    public class AssetsDatabaseInfo
    {
        public int TotalAssets { get; set; }
        public DateTime CreationDate { get; set; }
        public Dictionary<Guid, AssetInfo> Assets { get; private set; } = new();
    }
}
