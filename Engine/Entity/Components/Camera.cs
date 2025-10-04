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
    public struct FrustumCorners
    {
        public vec3 NearTopLeft;
        public vec3 NearTopRight;
        public vec3 NearBottomRight;
        public vec3 NearBottomLeft;

        public vec3 FarTopLeft;
        public vec3 FarTopRight;
        public vec3 FarBottomRight;
        public vec3 FarBottomLeft;

        public Bounds GetAABB()
        {
            vec3 min = NearTopLeft;
            vec3 max = NearTopLeft;

            void Include(vec3 p)
            {
                min.x = Math.Min(min.x, p.x);
                min.y = Math.Min(min.y, p.y);
                min.z = Math.Min(min.z, p.z);

                max.x = Math.Max(max.x, p.x);
                max.y = Math.Max(max.y, p.y);
                max.z = Math.Max(max.z, p.z);
            }

            Include(NearTopRight);
            Include(NearBottomRight);
            Include(NearBottomLeft);
            Include(FarTopLeft);
            Include(FarTopRight);
            Include(FarBottomRight);
            Include(FarBottomLeft);

            return new Bounds()
            {
                Min = min,
                Max = max
            };
        }
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

        private mat4 CreateOrtho(float size, bool matchWidth, float near, float far, vec4 viewport)
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

        private FrustumCorners GetOrthoFrustumCornersViewSpace(float left, float right,
                                                                     float bottom, float top,
                                                                     float near, float far)
        {
            FrustumCorners corners;

            // Near plane
            corners.NearTopLeft = new vec3(left, top, -near);
            corners.NearTopRight = new vec3(right, top, -near);
            corners.NearBottomRight = new vec3(right, bottom, -near);
            corners.NearBottomLeft = new vec3(left, bottom, -near);

            // Far plane
            corners.FarTopLeft = new vec3(left, top, -far);
            corners.FarTopRight = new vec3(right, top, -far);
            corners.FarBottomRight = new vec3(right, bottom, -far);
            corners.FarBottomLeft = new vec3(left, bottom, -far);

            return corners;
        }

        private FrustumCorners GetPerspectiveFrustumCornersViewSpace(float fov, float aspect,
                                                                           float near, float far)
        {
            FrustumCorners corners;

            float tanFov = MathF.Tan(glm.radians(fov) * 0.5f);

            float nh = near * tanFov;
            float nw = nh * aspect;
            float fh = far * tanFov;
            float fw = fh * aspect;

            // Near plane
            corners.NearTopLeft = new vec3(-nw, nh, -near);
            corners.NearTopRight = new vec3(nw, nh, -near);
            corners.NearBottomRight = new vec3(nw, -nh, -near);
            corners.NearBottomLeft = new vec3(-nw, -nh, -near);

            // Far plane
            corners.FarTopLeft = new vec3(-fw, fh, -far);
            corners.FarTopRight = new vec3(fw, fh, -far);
            corners.FarBottomRight = new vec3(fw, -fh, -far);
            corners.FarBottomLeft = new vec3(-fw, -fh, -far);

            return corners;
        }

        private FrustumCorners TransformFrustomCornersToWorld(FrustumCorners viewSpaceCorners, mat4 invView)
        {
            FrustumCorners world;
            world.NearTopLeft = new vec3(invView * new vec4(viewSpaceCorners.NearTopLeft, 1));
            world.NearTopRight = new vec3(invView * new vec4(viewSpaceCorners.NearTopRight, 1));
            world.NearBottomRight = new vec3(invView * new vec4(viewSpaceCorners.NearBottomRight, 1));
            world.NearBottomLeft = new vec3(invView * new vec4(viewSpaceCorners.NearBottomLeft, 1));
            world.FarTopLeft = new vec3(invView * new vec4(viewSpaceCorners.FarTopLeft, 1));
            world.FarTopRight = new vec3(invView * new vec4(viewSpaceCorners.FarTopRight, 1));
            world.FarBottomRight = new vec3(invView * new vec4(viewSpaceCorners.FarBottomRight, 1));
            world.FarBottomLeft = new vec3(invView * new vec4(viewSpaceCorners.FarBottomLeft, 1));
            return world;
        }

        public Bounds GetFrustumBoundsWorld()
        {
            FrustumCorners cornersVS;

            float aspect = Viewport.z / Viewport.w;

            if (ProjectionMode == CameraProjectionMode.Orthographic)
            {
                float size = OrthographicSize;
                float top = size * aspect;
                float bottom = -size * aspect;
                float right = size;
                float left = -size;

                cornersVS = GetOrthoFrustumCornersViewSpace(left, right, bottom, top, NearPlane, FarPlane);
            }
            else
            {
                cornersVS = GetPerspectiveFrustumCornersViewSpace(Fov, aspect, NearPlane, FarPlane);
            }

            FrustumCorners cornersWS = TransformFrustomCornersToWorld(cornersVS, ViewMatrix);
            return cornersWS.GetAABB();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Window.OnWindowChanged -= OnWindowChanged;
        }
    }
}
