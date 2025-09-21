using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    public class LayerEvent 
    {
        public EventType Type { get; internal set; }
    }

    public enum EventType
    {
        WindowFocusEnter,
        WindowFocusExit,
    }

    public abstract class LayerBase
    {
        //internal abstract int Priority { get; set; }
        public abstract void Initialize();
        public abstract void Close();
        internal virtual void UpdateLayer() { }
        public virtual void OnEvent(LayerEvent currentEvent) { }
    }
}
