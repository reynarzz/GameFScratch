using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public enum CurveMode
    {
        Linear,
        Smooth,
        Constant
    }

    public class AnimationCurve
    {
        private readonly List<Keyframe> _keys = new();
        public CurveMode Mode { get; set; } = CurveMode.Smooth;

        public AnimationCurve(params Keyframe[] keys)
        {
            _keys.AddRange(keys);
            _keys.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public void AddKey(Keyframe key)
        {
            _keys.Add(key);
            _keys.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public float Evaluate(float t)
        {
            if (_keys.Count == 0)
            {
                return 0f;
            }

            if (t <= _keys[0].Time)
            {
                return _keys[0].Value;
            }

            if (t >= _keys[^1].Time)
            {
                return _keys[^1].Value;
            }

            // Find segment
            for (int i = 0; i < _keys.Count - 1; i++)
            {
                var a = _keys[i];
                var b = _keys[i + 1];

                if (t >= a.Time && t <= b.Time)
                {
                    float dt = b.Time - a.Time;
                    float nt = (t - a.Time) / dt;

                    return Mode switch
                    {
                        CurveMode.Linear => Mathf.Lerp(a.Value, b.Value, nt),
                        CurveMode.Constant => a.Value,
                        CurveMode.Smooth => Hermite(a, b, nt, dt),
                        _ => a.Value
                    };
                }
            }

            return _keys[^1].Value;
        }

        private static float Hermite(Keyframe a, Keyframe b, float t, float dt)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float m0 = a.OutTangent * dt;
            float m1 = b.InTangent * dt;

            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            return h00 * a.Value + h10 * m0 + h01 * b.Value + h11 * m1;
        }

        public void SmoothTangents()
        {
            for (int i = 1; i < _keys.Count - 1; i++)
            {
                var prev = _keys[i - 1];
                var next = _keys[i + 1];
                var cur = _keys[i];

                float dv = next.Value - prev.Value;
                float dt = next.Time - prev.Time;
                float tangent = dv / dt;

                cur.InTangent = cur.OutTangent = tangent;
                _keys[i] = cur;
            }
        }
    }
}
