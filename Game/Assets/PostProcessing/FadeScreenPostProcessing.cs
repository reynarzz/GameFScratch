using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class FadeScreenPostProcessing : PostProcessingPass
    {
        protected override RenderTexture Render(RenderTexture inRenderTexture)
        {
            // TODO: Fade in/out 

            return inRenderTexture;
        }
    }
}