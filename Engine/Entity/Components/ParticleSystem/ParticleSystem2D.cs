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
        public vec2 VelocityMin { get; set; } = new(-1, -1);
        public vec2 VelocityMax { get; set; } = new(1, -1);
        public vec2 Spread { get; set; } = new(2.5f, 0.5f);
        public Color StartColor { get; set; } = Color.White;
        public Color EndColor { get; set; } = Color.Transparent;
        public vec2 StartSize { get; set; } = vec2.One;
        public vec2 EndSize { get; set; } = vec2.One;

        private List<Particle> _particles = new();
        private float _emitAccumulator = 0f;

        public vec2 Gravity { get; set; }
        public bool Prewarm { get; set; }
        internal override void OnInitialize()
        {
            base.OnInitialize();

            Mesh = new Mesh();
            Mesh.IndicesToDrawCount = 0;
            const float bufferOffset = 1.2f;

            _particles.Capacity = (int)MathF.Ceiling(EmitRate * ParticleLife * bufferOffset);


            ////-----------------
            //for (int i = 0; i < EmitRate * ParticleLife * 14; i++) // Remove this
            //{
            //    Mesh.Vertices.Add(default); // Remove this
            //}
            //Mesh.IndicesToDrawCount = Mesh.Vertices.Count / 4 * 6; // Remove this
            ////------------------
        }

        public void OnUpdate()
        {
            for (int i = _particles.Count - 1; i >= 0; --i)
            {
                var particle = _particles[i];
                particle.Life -= Time.DeltaTime;

                if (particle.Life <= 0)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                float time = 1.0f - (particle.Life / particle.StartLife);

                particle.Velocity += Gravity * Time.DeltaTime;
                particle.Position += particle.Velocity * Time.DeltaTime;
                particle.Rotation += particle.AngularVelocity * Time.DeltaTime;
                particle.Color = Color.Lerp(StartColor, EndColor, time);
                particle.Size = Mathf.Lerp(StartSize, EndSize, time);

                _particles[i] = particle;
            }

            _emitAccumulator += Time.DeltaTime * EmitRate;

            while (_emitAccumulator >= 1.0f)
            {
                EmitParticle();
                _emitAccumulator -= 1.0f;
            }
        }

        private void EmitParticle()
        {
            //if (_aliveCount >= _particles.Count)
            //    return;
            Debug.Log("Emit");
            var particle = new Particle()
            {
                Color = StartColor,
                StartLife = ParticleLife,
                Life = ParticleLife,
                Position = new vec2(RandomFloat(-Spread.x, Spread.x), RandomFloat(-Spread.y, Spread.y)),
                Rotation = 0,
                Velocity = new vec2(RandomFloat(VelocityMin.x, VelocityMax.x), RandomFloat(VelocityMin.y, VelocityMax.y)),
                AngularVelocity = RandomFloat(-2.0f, 2.0f),
                Size = StartSize
            };

            _particles.Add(particle);
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

            for (int i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];

                var particleModel = Transform.WorldMatrix *
                                    glm.translate(mat4.identity(), particle.Position) *
                                    glm.rotate(glm.radians(particle.Rotation), new vec3(0, 0, 1));

                var size = particle.Size;

                QuadVertices quad = default;
                GraphicsHelper.CreateQuad(ref quad, chunk.Uvs, size.x, size.y, chunk.Pivot, particle.Color, particleModel);

                int baseIndex = i * 4;

                // Resize if needed
                while (Mesh.Vertices.Count < baseIndex + 4)
                {
                    Mesh.Vertices.Add(default);
                }
                //Debug.Log(particle.Position);
                // Debug.DrawBox(new vec3(particleModel[3][0], particleModel[3][1]), size, particle.Color);

                // Update vertex data
                Mesh.Vertices[baseIndex + 0] = quad.v0;
                Mesh.Vertices[baseIndex + 1] = quad.v1;
                Mesh.Vertices[baseIndex + 2] = quad.v2;
                Mesh.Vertices[baseIndex + 3] = quad.v3;
            }

            Mesh.IndicesToDrawCount = _particles.Count * 6;
            IsDirty = true;
        }
    }
}