using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Material : EObject
    {
        public Shader Shader { get; }
        private List<Texture> _tex;

        public Material(Shader shader)
        {
            Shader = shader;
            _tex = new List<Texture>();
        }

        public Texture GetTexture(int index) 
        {
            return _tex[index];
        }
    }
}