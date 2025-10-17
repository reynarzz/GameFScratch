using Engine;
using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class CameraFollow : ScriptBehavior
    {
        public Transform Target { get; set; }
        public float FollowSpeed { get; set; } = 5f;

        private vec2 deadZoneSize = new vec2(0f, 4f);
        private float smoothTime = 0.3f;
        private vec3 velocity;

        public override void OnLateUpdate()
        {
            Move();
        }

        private void Move()
        {
            if (!Target)
                return;

            var camPos = Transform.WorldPosition;
            var targetPos = Target.WorldPosition;

            // Dead zone boundaries relative to camera center
            float left = camPos.x - deadZoneSize.x * 0.5f;
            float right = camPos.x + deadZoneSize.x * 0.5f;
            float bottom = camPos.y - deadZoneSize.y * 0.5f;
            float top = camPos.y + deadZoneSize.y * 0.5f;

            float newX = camPos.x;
            float newY = camPos.y;

            // Check horizontal
            if (targetPos.x < left) newX = targetPos.x + deadZoneSize.x * 0.5f;
            if (targetPos.x > right) newX = targetPos.x - deadZoneSize.x * 0.5f;

            // Check vertical
            if (targetPos.y < bottom) newY = targetPos.y + deadZoneSize.y * 0.5f;
            if (targetPos.y > top) newY = targetPos.y - deadZoneSize.y * 0.5f;

            var targetCameraPos = new vec3(newX, newY, camPos.z);

            Transform.WorldPosition = Mathf.SmoothDamp(camPos, targetCameraPos, ref velocity, smoothTime);

            if (Physics2D.DrawColliders)
            {
                Debug.DrawBox(new vec3(Transform.WorldPosition.x, Transform.WorldPosition.y, 0), new vec3(deadZoneSize.x, deadZoneSize.y, 0), Color.Green);
            }
        }
    }
}