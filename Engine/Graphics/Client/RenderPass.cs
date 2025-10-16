using Engine.Graphics;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class RenderPass
    {
        private readonly Dictionary<string, UniformValue> _uniforms;
        internal IReadOnlyDictionary<string, UniformValue> Uniforms => _uniforms;

        public bool IsScreenGrabPass { get; set; }
        public Shader Shader { get; }
        public Blending Blending { get; } = new()
        {
            Enabled = true,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Equation = BlendEquation.FuncAdd
        };

        public Stencil Stencil { get; } = new();

        internal RenderPass(Shader shader)
        {
            Shader = shader;
            _uniforms = new Dictionary<string, UniformValue>();
        }

        // Generic SetProperty uses precomputed Setter
        internal void SetProperty<T>(string name, T value) where T : unmanaged
        {
            UniformSetter<T>.Setter(this, name, value);
        }

        private static class UniformSetter<T> where T : unmanaged
        {
            public static readonly Action<RenderPass, string, T> Setter;

            static UniformSetter()
            {
                if (typeof(T) == typeof(int))
                {
                    Setter = (pass, name, value) =>
                        pass._uniforms[name] = UniformValue.AsInt(name, Unsafe.As<T, int>(ref value));
                }
                else if (typeof(T) == typeof(float))
                {
                    Setter = (pass, name, value) =>
                        pass._uniforms[name] = UniformValue.AsFloat(name, Unsafe.As<T, float>(ref value));
                }
                else if (typeof(T) == typeof(vec2))
                {
                    Setter = (pass, name, value) =>
                        pass._uniforms[name] = UniformValue.AsVec2(name, Unsafe.As<T, vec2>(ref value));
                }
                else if (typeof(T) == typeof(vec3))
                {
                    Setter = (pass, name, value) =>
                        pass._uniforms[name] = UniformValue.AsVec3(name, Unsafe.As<T, vec3>(ref value));
                }
                else if (typeof(T) == typeof(mat4))
                {
                    Setter = (pass, name, value) =>
                        pass._uniforms[name] = UniformValue.AsMat4(name, Unsafe.As<T, mat4>(ref value));
                }
                else
                {
                    throw new NotSupportedException($"Uniform type {typeof(T)} is not supported");
                }
            }
        }

        
    }

}
