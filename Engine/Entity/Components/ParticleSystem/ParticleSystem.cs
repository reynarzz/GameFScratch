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
    public class ParticleSystem : Component, IUpdatableComponent
    {
        public Texture2D Texture { get; set; }
        public float EmitRate { get; set; } = 30f;
        public float ParticleLife { get; set; } = 1.5f;
        public vec2 VelocityMin { get; set; } = new(-20, -50);
        public vec2 VelocityMax { get; set; } = new(20, -80);
        public vec2 Spread { get; set; } = new(0.5f, 0.5f);
        public Color StartColor { get; set; } = Color.White;
        public Color EndColor { get; set; } = Color.Transparent;

        private List<Particle> _particles = new();
        private float _emitAccumulator = 0f;

        public override void OnEnabled()
        {
            // _particles.Capacity = 
        }

        public void OnUpdate()
        {
            _emitAccumulator += Time.DeltaTime * EmitRate;

            while (_emitAccumulator >= 1.0f)
            {
                EmitParticle();
                _emitAccumulator -= 1.0f;
            }

            for (int i = _particles.Count-1; i >= 0; --i)
            {
                var particle = _particles[i];
                particle.Life -= Time.DeltaTime;
                if(particle.Life <= 0)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                float time = 1.0f - (particle.Life / particle.StartLife);

                particle.Position += particle.Velocity * Time.DeltaTime;
                particle.Rotation += particle.AngularVelocity * Time.DeltaTime;
                particle.Color = Color.Lerp(StartColor, EndColor, time);
                _particles[i] = particle;
            }
        }

        private void EmitParticle()
        {
            var particle = new Particle()
            {
                Color = StartColor,
                StartLife = ParticleLife,
                Life = ParticleLife,
                Position = new vec2(RandomFloat(-Spread.x, Spread.x), RandomFloat(-Spread.y, Spread.y)),
                Rotation = 0,
                Velocity = new vec2(RandomFloat(-VelocityMin.x, VelocityMax.x), RandomFloat(-VelocityMin.y, VelocityMax.y)),
                AngularVelocity = RandomFloat(-2.0f, 2.0f),
            };

            _particles.Add(particle);
        }


        private float RandomFloat(float min, float max)
        {
            return (float)(Random.Shared.NextDouble() * (max - min) + min);
        }
    }
}