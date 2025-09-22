using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utils;
using GlmSharp;

namespace Engine
{
    public class Camera : Component
    {
        public mat4 Projection { get; private set; }
        public mat4 ViewMatrix => MathUtils.InvertCameraTransform(Transform.WorldMatrix);
        public int Priority { get; set; } = 0;
        public PerspectiveConfig PerspectiveConfig { get; set; }
        public OrthographicConfig OrthoConfig { get; set; }
        public float NearPlane { get; set; } = 1;
        public float FarPlane { get; set; } = 100;
        public vec4 BackgroundColor { get; set; } = new(1, 1, 1, 1);

        public vec4 _viewport;
        public vec4 Viewport
        {
            get
            {
                _viewport.z = Window.Width;
                _viewport.w = Window.Height;

                return _viewport;
            }
            set
            {
                _viewport.x = value.x;
                _viewport.y = value.y;
            }
        }

        public Camera()
        {
            OrthoConfig = new() { OrthographicSize = 32 };
            PerspectiveConfig = new() { Fov = 60.0f, AspectRatio = 1 };

            UpdateOrthographic();
        }

        private void UpdateOrthographic()
        {
            float aspectRatio = Viewport.z / Viewport.w;

            var size = OrthoConfig.OrthographicSize;

            Projection = MathUtils.Ortho(-size * aspectRatio, size * aspectRatio, -size, size,
                                                 NearPlane, FarPlane);
        }
    }

    public struct PerspectiveConfig
    {
        public float Fov { get; set; }
        public float AspectRatio { get; set; }
    }

    public struct OrthographicConfig
    {
        public float OrthographicSize { get; set; }
    }

}
