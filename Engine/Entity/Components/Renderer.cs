using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Base class for all the renderers
    /// </summary>
    public abstract class Renderer : Component
    {
        public Material Material { get; set; }
        public Mesh Mesh { get; set; }

        internal abstract void Initialize();
    }
}