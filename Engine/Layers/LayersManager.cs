using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Layers
{
    internal class LayersManager
    {
        private LayerBase[] _layers;
        public LayersManager(Type[] layersTypes)
        {
            _layers = new LayerBase[layersTypes.Length];
            for (int i = 0; i < layersTypes.Length; i++)
            {
                _layers[i] = (LayerBase)Activator.CreateInstance(layersTypes[i]);
            }
        }

        internal void Initialize()
        {
            for (int i = _layers.Length-1; i >= 0; i--)
            {
                _layers[i].Initialize();
            }
        }

        internal void Update()
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                _layers[i].UpdateLayer();
            }
        }

        internal void PublishEvent(LayerEvent currentEvent) 
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                _layers[i].OnEvent(currentEvent);
            }
        }
    }
}
