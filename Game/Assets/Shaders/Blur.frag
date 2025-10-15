#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;

flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

void main()
{
    // Pixel size in UV space
    vec2 texel = 1.0 / uScreenSize;

    // Blur radius (in pixels)
    float radius = 7.0;

    // Accumulator
    vec3 color = vec3(0.0);
    float count = 0.0;

    // Simple box blur (symmetric 7x7 kernel)
    for (int x = -3; x <= 3; x++)
    {
        for (int y = -3; y <= 3; y++)
        {
            vec2 offset = vec2(float(x), float(y)) * texel * radius * 0.15;
            color += texture(uScreenGrabTex, screenUV + offset).rgb;
            count += 1.0;
        }
    }

    // Average color
    color /= count;

    fragColor = vec4(color, 1.0) * vColor;
}

