using Engine.Graphics;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Material : EObject
    {
        private List<RenderPass> _passes;
        public IReadOnlyCollection<RenderPass> Passes => _passes;
        private Dictionary<string, Texture> _textures;
        internal IReadOnlyDictionary<string, Texture> Textures => _textures;

        public Material(Shader shader)
        {
            _textures = new Dictionary<string, Texture>();
            _passes = new List<RenderPass>()
            {
                new RenderPass(shader)
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

        public void AddTexture(string name, Texture texture)
        {
            _textures[name] = texture;
        }

        public void SetProperty<T>(int pass, string name, T value) where T: unmanaged
        {
            if(GetPassSafe(pass, out var passObj))
            {
                passObj.SetProperty(name, value);
            }
        }

        private bool GetPassSafe(int index, out RenderPass pass)
        {
            pass = null;

            if (_passes.Count > index)
            {
                pass = _passes[index];
                return true;
            }

            Debug.Error($"Render pass index is out of range: {index}");

            return false;
        }
    }
}