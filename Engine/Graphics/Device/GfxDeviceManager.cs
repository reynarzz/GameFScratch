using Engine.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Graphics
{
    // TODO: Factory device class 
    internal class GfxDeviceManager
    {
        private static GfxDevice _device;
        public static GfxDevice Current => _device;

        static GfxDeviceManager()
        {
            _device = new GLGfxDevice();
            _device.Initialize();
        }

    }
}