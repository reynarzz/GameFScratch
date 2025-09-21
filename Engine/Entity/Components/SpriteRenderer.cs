using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class SpriteRenderer : Renderer
    {
        public int SortingOrder { get; set; } = 0;

        internal override void Initialize()
        {
        }

        protected override void OnMaterialSet(Material mat)
        {
            if (!mat)
            {
                // Look
            }
        }

        internal override GfxResource GetGeometry()
        {
            // TODO:
            return null;
        }

    }
}
