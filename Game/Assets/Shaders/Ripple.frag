#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;
flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

uniform vec2 uRippleCenter = vec2(0.5);
uniform float uRippleSpeed = 2.0;
uniform float uRippleAmplitude = 0.02;
uniform float uRippleFrequency = 20.0;

void main()
{
    vec2 dir = screenUV - uRippleCenter;
    float dist = length(dir);

    // Ripple distortion
    float offset = sin(dist * uRippleFrequency - uTime.x * uRippleSpeed) * uRippleAmplitude;
    vec2 uv = screenUV + normalize(dir) * offset;

    vec3 color = texture(uScreenGrabTex, uv).rgb;

    fragColor = vec4(color, 1.0) * vColor;
}
