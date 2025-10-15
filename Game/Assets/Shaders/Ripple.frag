#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;
flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

// Ripple settings
uniform vec2  uRippleCenter    = vec2(0.5); // Center point
uniform float uRippleSpeed     = 2.0;       // Wave speed
uniform float uRippleAmplitude = 0.02;      // Max displacement
uniform float uRippleFrequency = 20.0;      // Wave frequency
uniform float uEdgeStart       = 0.3;       // Start of ripple (0 = center, 1 =e dge)
uniform float uEdgeEnd         = 0.7;       // Full ripple at edge

void main()
{
    vec2 dir  = screenUV - uRippleCenter;
    float dist = length(dir);

    // Compute how much ripple applies (fade near center)
    // 0 at center, 1 at edges
    float edgeFactor = smoothstep(uEdgeStart, uEdgeEnd, dist);

    // Ripple offset — only strong at edges
    float ripple = sin(dist * uRippleFrequency - uTime.x * uRippleSpeed) * uRippleAmplitude * edgeFactor;

    // Distortion direction
    vec2 uv = screenUV + normalize(dir) * ripple;

    // Sample final color
    vec3 color = texture(uScreenGrabTex, uv).rgb;

    fragColor = vec4(color, 1.0) * vColor;
}
