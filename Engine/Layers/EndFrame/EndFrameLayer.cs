using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class EndFrameLayer : LayerBase
    {
        public static bool CleaningUp { get; private set; }

        public override void Initialize()
        {
        }

        internal override void UpdateLayer()
        {
            CleaningUp = true;
            SceneManager.ActiveScene.DeletePending();
            CleaningUp = false;
        }

        public override void Close()
        {
        }

    }
}
