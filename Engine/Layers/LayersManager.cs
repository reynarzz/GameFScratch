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
        private CleanUpLayer _cleanupLayer;
        public LayersManager(Type[] layersTypes)
        {
            _layers = new LayerBase[layersTypes.Length];
            for (int i = 0; i < layersTypes.Length; i++)
            {
                _layers[i] = (LayerBase)Activator.CreateInstance(layersTypes[i]);
            }

            _cleanupLayer = new CleanUpLayer();
        }

        internal void Initialize()
        {
            _cleanupLayer.Initialize();

            for (int i = _layers.Length - 1; i >= 0; i--)
            {
                _layers[i].Initialize();

//#if DEBUG
//                try
//                {
//                    _layers[i].Initialize();
//                }
//                catch (Exception e)
//                {
//                    Debug.Error(e);
//                }
//#else
//                _layers[i].Initialize();
//#endif
            }
        }

        internal void Update()
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                _layers[i].UpdateLayer();
            }

            _cleanupLayer.UpdateLayer();
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
