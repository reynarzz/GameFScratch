using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Material : EObject
    {
        private List<Texture> _tex;
        public Texture GetTexture(int index) 
        {
            return _tex[index];
        }
    }
}