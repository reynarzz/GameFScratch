using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class Time
    {
        public static float DeltaTime { get; internal set; } = 0f;
        public static float FPS { get; internal set; } = 0f;
        public static float SinceStarted { get; internal set; } = 0f;
        public static float TimeScale { get; internal set; } = 1f;
        public static float TimeCurrent { get; internal set; } = 0;
    }
}
