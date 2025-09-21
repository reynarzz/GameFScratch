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
        private Material _material;
        public Material Material 

        { 
            get => _material;
            set
            {
                if(_material == value)
                {
                    return;
                }

                _material = value;
                OnMaterialSet(_material);
            }
        }

        protected virtual void OnMaterialSet(Material mat) { }
        internal abstract void Initialize();
        internal abstract GfxResource GetGeometry();
    }
}