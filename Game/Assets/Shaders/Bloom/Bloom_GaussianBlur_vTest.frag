#version 330 core

in vec2 screenUV;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;   // (width, height)
uniform bool uHorizontal;   // true = horizontal, false = vertical

void main()
{
    // Convert to UV texel size
    vec2 texelSize = 1.0 / uScreenSize;

    // Proper Gaussian weights for radius 9 (σ ≈ 4.0)
    float weights[9] = float[](
        0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216,
        0.005, 0.002, 0.001, 0.0005
    );

    vec3 result = texture(uScreenGrabTex, screenUV).rgb * weights[0];

    for (int i = 1; i < 9; i++)
    {
        vec2 offset = uHorizontal
            ? vec2(texelSize.x * float(i), 0.0)
            : vec2(0.0, texelSize.y * float(i));

        result += texture(uScreenGrabTex, screenUV + offset).rgb * weights[i];
        result += texture(uScreenGrabTex, screenUV - offset).rgb * weights[i];
    }

    fragColor = vec4(result, 1.0);
}
