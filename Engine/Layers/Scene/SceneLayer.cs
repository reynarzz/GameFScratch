using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class SceneLayer : LayerBase
    {
        public override void Close()
        {
        }

        public override void Initialize()
        {
        }

        internal override void UpdateLayer()
        {
            SceneManager.ActiveScene.Update();
            SceneManager.ActiveScene.LateUpdate();
        }
    }
}
