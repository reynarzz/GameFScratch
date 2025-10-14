using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class RenderPass
    {
        public bool IsScreenGrabPass { get; set; }
        public Shader Shader { get; set; }
        public Blending Blending { get; } = new Blending()
        {
            Enabled = true,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Equation = BlendEquation.FuncAdd
        };

        public Stencil Stencil { get; } = new Stencil();
    }

    public class Material : EObject
    {
        public RenderPass MainPass { get; }
        private List<RenderPass> _passes;
        public IReadOnlyCollection<RenderPass> Passes => _passes;
        private List<Texture> _textures;
        public List<Texture> Textures => _textures;
        public Material(Shader shader)
        {
            MainPass = new RenderPass() { Shader = shader };

            _textures = new List<Texture>();
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
    }
}