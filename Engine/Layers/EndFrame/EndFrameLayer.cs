using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class EndFrameLayer : LayerBase
    {
        public override void Initialize()
        {
        }

        internal override void UpdateLayer()
        {
            SceneManager.ActiveScene.DeletePending();
        }

        public override void Close()
        {
        }

    }
}
