using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Utils;
using GlmNet;

namespace Engine
{
    public enum CameraProjectionMode
    {
        Orthographic,
        Perspective
    }

    public enum CameraOrthoMatch
    {
        Height,
        Width
    }

    public class Camera : Component
    {
        public mat4 Projection { get; private set; }
        public mat4 ViewMatrix => glm.inverse(Transform.WorldMatrix);
        public int Priority { get; set; } = 0;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100;
        public Color BackgroundColor { get; set; } = new(1, 1, 1, 1);
        private CameraOrthoMatch _orthoMatch = CameraOrthoMatch.Width;
        public RenderTexture RenderTexture { get; set; }

        public CameraOrthoMatch OrthoMatch
        {
            get => _orthoMatch;
            set
            {
                if (_orthoMatch == value)
                    return;

                _orthoMatch = value;
                UpdateCurrent();
            }
        }

        private CameraProjectionMode _projectionMode;
        public CameraProjectionMode ProjectionMode
        {
            get => _projectionMode;
            set
            {
                if (_projectionMode == value)
                    return;

                _projectionMode = value;
                UpdateCurrent();
            }
        }

        private float _orthoSize;
        public float OrthographicSize
        {
            get => _orthoSize;
            set
            {
                _orthoSize = value;
                UpdateCurrent();
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
            UpdateCurrent();
            //UpdatePerspective();
        }

        private void OnWindowChanged(int width, int height)
        {
            UpdateCurrent();
        }

        private void UpdateCurrent()
        {
            if (_projectionMode == CameraProjectionMode.Orthographic)
            {
                var matchWidth = _orthoMatch == CameraOrthoMatch.Width;
                var orthoSize = OrthographicSize;

                if (matchWidth)
                {
                    //orthoSize = OrthographicSize / (Viewport.w / Viewport.z);
                }
                Projection = CreateOrtho(orthoSize, matchWidth, NearPlane, FarPlane, Viewport);
            }
            else
            {
                float aspectRatio = Viewport.z / Viewport.w;
                Projection = MathUtils.Perspective(Fov, aspectRatio, NearPlane, FarPlane);
            }
        }

        public static mat4 CreateOrtho(float size, bool matchWidth, float near, float far, vec4 viewport)
        {
            float left, right, bottom, top, aspect;

            if (matchWidth)
            {
                aspect = viewport.w / viewport.z;
                bottom = -size * aspect;
                top = size * aspect;
                left = -size;
                right = size;
            }
            else
            {
                aspect = viewport.z / viewport.w;
                bottom = -size;
                top = size;
                left = -size * aspect;
                right = size * aspect;
            }

            return MathUtils.Ortho(left, right, bottom, top, near, far);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Window.OnWindowChanged -= OnWindowChanged;
        }
    }
}
