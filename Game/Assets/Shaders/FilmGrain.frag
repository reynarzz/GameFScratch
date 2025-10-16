#version 330 core

in vec2 screenUV;
in vec4 vColor;
flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform float uFrameSeed;       
uniform float uNoiseStrength = 0.1;
uniform float uNoiseSize = 1.0;

// Integer-style hash (no repeating patterns)
float random(vec2 uv, float seed)
{
    // Convert pixel position to integer grid
    ivec2 i = ivec2(floor(uv * uScreenSize / uNoiseSize));
    uint n = uint(i.x * 1973 + i.y * 9277) ^ uint(seed * 1e6);
    n = n * 1597334677u ^ (n >> 13);
    return float(n & 0x00FFFFFFu) / float(0x01000000u);
}

void main()
{
    vec3 color = texture(uScreenGrabTex, screenUV).rgb;

    // Generate fully decorrelated random per pixel and frame
    float nR = random(screenUV, uFrameSeed + 1.0);
    float nG = random(screenUV + vec2(1.0, 0.0), uFrameSeed + 2.0);
    float nB = random(screenUV + vec2(0.0, 1.0), uFrameSeed + 3.0);

    vec3 noise = vec3(nR, nG, nB) - 0.5;
    color += noise * uNoiseStrength;

    fragColor = vec4(color, 1.0) * vColor;
}
