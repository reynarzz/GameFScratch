using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCooker
{
    [Serializable]
    public struct ProjectConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProjectFolderRoot { get; set; }
    }
}
