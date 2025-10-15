#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;
flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

uniform float uVignetteStrength = 0.5; // 0 = no vignette, 1 = full dark edges

void main()
{
    vec3 color = texture(uScreenGrabTex, screenUV).rgb;

    // Distance from screen center
    vec2 center = vec2(0.5, 0.5);
    float dist = distance(screenUV, center);

    // Smooth radial vignette
    float vignette = 1.0 - smoothstep(0.0, 0.5, dist); 
    vignette = mix(1.0, vignette, uVignetteStrength);

    color *= vignette;

    fragColor = vec4(color, 1.0) * vColor;
}
