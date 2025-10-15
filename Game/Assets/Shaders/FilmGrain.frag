#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;
flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

// Controls
uniform float uNoiseStrength = 0.3; // intensity of grain

// Better pseudo-random hash
float rand(vec2 co)
{
    return fract(sin(dot(co ,vec2(12.9898,78.233))) * 43758.5453);
}

void main()
{
    vec3 color = texture(uScreenGrabTex, screenUV).rgb;

    // Fully random noise per pixel, using UV and time
    float nR = rand(screenUV * uTime.x * 1000.0);
    float nG = rand((screenUV + vec2(1.0,0.0)) * uTime.x * 1000.0);
    float nB = rand((screenUV + vec2(0.0,1.0)) * uTime.x * 1000.0);

    color.r += (nR - 0.5) * uNoiseStrength;
    color.g += (nG - 0.5) * uNoiseStrength;
    color.b += (nB - 0.5) * uNoiseStrength;

    fragColor = vec4(color, 1.0) * vColor;
}
