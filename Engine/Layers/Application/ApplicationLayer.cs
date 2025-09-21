using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    public abstract class ApplicationLayer : LayerBase
    {
        public abstract string GameName { get; }
        public virtual void OnFocusEnter() { }
        public virtual void OnFocusExit() { }
    }
}