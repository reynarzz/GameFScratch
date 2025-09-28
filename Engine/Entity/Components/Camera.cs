using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utils;
using GlmNet;

namespace Engine
{
    public class Camera : Component
    {
        public mat4 Projection { get; private set; }
        public mat4 ViewMatrix => glm.inverse(Transform.WorldMatrix);
        public int Priority { get; set; } = 0;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100;
        public Color BackgroundColor { get; set; } = new(1, 1, 1, 1);

        private float _orthoSize;
        public float OrthographicSize { get => _orthoSize; 
            set 
            {
                _orthoSize = value;
                UpdateOrthographic();
            } 
        }

        public float Fov { get; set; }

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
            OrthographicSize = 32;
            Fov = 60.0f;
            Window.OnWindowChanged += OnWindowChanged;
            UpdateOrthographic();
            //UpdatePerspective();
        }

        private void OnWindowChanged(int width, int height)
        {
            UpdateOrthographic();
        }

        private void UpdateOrthographic()
        {
            float aspectRatio = Viewport.z / Viewport.w;

            var size = OrthographicSize;
            Projection = MathUtils.Ortho(-size * aspectRatio, size * aspectRatio, -size, size,
                                                 NearPlane, FarPlane);
        }

        private void UpdatePerspective()
        {
            float aspectRatio = Viewport.z / Viewport.w;
            Projection = MathUtils.Perspective(Fov, aspectRatio, NearPlane, FarPlane);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Window.OnWindowChanged -= OnWindowChanged;
        }
    }
}
