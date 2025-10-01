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
        public Graphics.Blending Blending { get; set; } = new Graphics.Blending() {  Enabled = true };
        public Graphics.Stencil Stencil { get; set; }

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