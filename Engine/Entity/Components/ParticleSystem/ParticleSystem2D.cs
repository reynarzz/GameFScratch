using GlmNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class ParticleSystem2D : Renderer2D, IUpdatableComponent
    {
        public float EmitRate { get; set; } = 30f;
        public float ParticleLife { get; set; } = 1.5f;
        public vec2 VelocityMin { get; set; } = new(-20, -50);
        public vec2 VelocityMax { get; set; } = new(20, -80);
        public vec2 Spread { get; set; } = new(0.5f, 0.5f);
        public Color StartColor { get; set; } = Color.White;
        public Color EndColor { get; set; } = Color.Transparent;
        public vec2 StartSize { get; set; } = vec2.One;
        public vec2 EndSize { get; set; } = vec2.One;

        private List<Particle> _particles = new();
        private float _emitAccumulator = 0f;
        private int _aliveCount;

        public vec2 Gravity { get; set; }
        public bool Prewarm { get; set; }
        internal override void OnInitialize()
        {
            base.OnInitialize();

            Mesh = new Mesh();
            Mesh.IndicesToDrawCount = 0;
            const float bufferOffset = 1.2f;

            _particles.Capacity = (int)MathF.Ceiling(EmitRate * ParticleLife * bufferOffset);


            QuadVertices vertices = default;
                Mesh.Vertices.Add(vertices.v0);
                Mesh.Vertices.Add(vertices.v1);
                Mesh.Vertices.Add(vertices.v2);
                Mesh.Vertices.Add(vertices.v3);


                if (Prewarm)
                PrewarmSystem();
        }

        public void OnUpdate()
        {
            _emitAccumulator += Time.DeltaTime * EmitRate;

            while (_emitAccumulator >= 1.0f)
            {
                EmitParticle();
                _emitAccumulator -= 1.0f;
            }

            for (int i = _particles.Count - 1; i >= 0; --i)
            {
                var particle = _particles[i];
                particle.Life -= Time.DeltaTime;

                if (particle.Life <= 0)
                {
                    _particles.RemoveAt(i);
                    _aliveCount--;
                    continue;
                }

                float time = 1.0f - (particle.Life / particle.StartLife);

                particle.Velocity += Gravity * Time.DeltaTime;
                particle.Position += particle.Velocity * Time.DeltaTime;
                particle.Rotation += particle.AngularVelocity * Time.DeltaTime;
                particle.Color = Color.Lerp(StartColor, EndColor, time);
                // particle.Size = Mathf.Lerp(StartSize, EndSize, time);

                _particles[i] = particle;
            }
        }

        private void EmitParticle()
        {
            //if (_aliveCount >= _particles.Count)
            //    return;

            var particle = new Particle()
            {
                Color = StartColor,
                StartLife = ParticleLife,
                Life = ParticleLife,
                Position = new vec2(RandomFloat(-Spread.x, Spread.x), RandomFloat(-Spread.y, Spread.y)),
                Rotation = 0,
                Velocity = new vec2(RandomFloat(-VelocityMin.x, VelocityMax.x), RandomFloat(-VelocityMin.y, VelocityMax.y)),
                AngularVelocity = RandomFloat(-2.0f, 2.0f),
                Size = StartSize
            };

            _aliveCount++;

            _particles.Add(particle);
        }

        private void PrewarmSystem()
        {
            int count = (int)MathF.Ceiling(EmitRate * ParticleLife);
            for (int i = 0; i < count; i++)
            {
                EmitParticle();

                var p = _particles[_aliveCount - 1];
                p.Life = RandomFloat(0, ParticleLife);
                _particles[_aliveCount - 1] = p;
            }
        }

        private float RandomFloat(float min, float max)
        {
            return (float)(Random.Shared.NextDouble() * (max - min) + min);
        }

        internal void Render()
        {
            var texture = Sprite.Texture;
            var chunk = texture.Atlas.GetChunk(0);
            float ppu = texture.PixelPerUnit;

            if(_particles.Count > 0)
            Mesh.Vertices.Clear(); // remove
            Mesh.IndicesToDrawCount = 0;
            
            for (int i = 0; i < _particles.Count; i++)
            {
                var particleModel = Transform.WorldMatrix * glm.translate(mat4.identity(), _particles[i].Position) * glm.rotate(glm.radians(_particles[i].Rotation), new vec3(0, 0, 1));
                var size = _particles[i].Size;

                QuadVertices vertices = default;
                GraphicsHelper.CreateQuad(ref vertices, chunk.Uvs, size.x, size.y, chunk.Pivot, _particles[i].Color, particleModel);

                //if(Mesh.Vertices.Count > i * 4)
                //{
                //    Mesh.Vertices[i + 0] = vertices.v0;
                //    Mesh.Vertices[i + 1] = vertices.v1;
                //    Mesh.Vertices[i + 2] = vertices.v2;
                //    Mesh.Vertices[i + 3] = vertices.v3;
                //}
                //else
                {
                    Mesh.Vertices.Add(vertices.v0);
                    Mesh.Vertices.Add(vertices.v1);
                    Mesh.Vertices.Add(vertices.v2);
                    Mesh.Vertices.Add(vertices.v3);
                }

                Mesh.IndicesToDrawCount += 6;
            }

            if(Mesh.IndicesToDrawCount > 0)
            {
                IsDirty = true;
            }
            else {
                IsDirty = false;
            }
            Debug.Log("Particle system: " + _particles.Count + ", " + Mesh.IndicesToDrawCount);
        }
    }
}