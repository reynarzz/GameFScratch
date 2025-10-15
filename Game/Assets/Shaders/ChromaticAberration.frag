#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;

flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

// Optional: strength multiplier for aberration
uniform float uAberrationStrength = 0.008;

void main()
{
    // Compute normalized coordinates from screen center (-0.5..0.5)
    vec2 center = vec2(0.5);
    vec2 toCenter = screenUV - center;

    // Radial distance for falloff
    float dist = length(toCenter);

    // Smooth falloff function for edge effect
    float falloff = smoothstep(0.0, 0.7, dist);

    // Compute offsets per channel (R outward, B inward, G stays)
    vec2 offsetR = toCenter * uAberrationStrength * falloff;
    vec2 offsetB = -toCenter * uAberrationStrength * falloff;
    vec2 offsetG = vec2(0.0);

    // Optional: slightly sample nearby pixels for smooth edges (soft chromatic blur)
    float blurScale = 0.5; // lower = sharper
    vec3 colR = texture(uScreenGrabTex, screenUV + offsetR * blurScale).rgb;
    vec3 colG = texture(uScreenGrabTex, screenUV + offsetG * blurScale).rgb;
    vec3 colB = texture(uScreenGrabTex, screenUV + offsetB * blurScale).rgb;

    // Combine RGB channels
    vec3 color = vec3(colR.r, colG.g, colB.b);

    fragColor = vec4(color, 1.0) * vColor;
}
