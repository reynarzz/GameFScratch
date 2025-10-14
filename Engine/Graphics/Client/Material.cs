using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class RenderPass
    {
        public int Order { get; set; }
        public bool IsScreenGrabPass { get; set; }
        public Shader Shader { get; set; }
        public Graphics.Blending Blending { get; set; } = new Graphics.Blending() { Enabled = true };
        public Graphics.Stencil Stencil { get; set; }
    }

    public class Material : EObject
    {
        public RenderPass MainPass { get; }
        private List<RenderPass> _passes;
        public IReadOnlyCollection<RenderPass> Passes => _passes;
        private List<Texture> _tex;

        public Material(Shader shader)
        {
            MainPass = new RenderPass() { Order = 0, Shader = shader };

            _tex = new List<Texture>();
            _passes = new List<RenderPass>()
            {
                MainPass
            };
        }

        public void AddPass(RenderPass pass)
        {
            _passes.Add(pass);
        }

        public void RemovePass(RenderPass pass)
        {
            _passes.Remove(pass);
        }

        public void RemovePass(int index)
        {
            _passes.RemoveAt(index);
        }

        public Texture GetTexture(int index)
        {
            return _tex[index];
        }
    }
}