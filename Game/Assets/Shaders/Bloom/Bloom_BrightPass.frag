#version 330 core

in vec2 screenUV;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform float uThreshold = 0.85;

void main()
{
    vec3 color = texture(uScreenGrabTex, screenUV).rgb;
    float brightness = max(max(color.r, color.g), color.b);
    color = (brightness > uThreshold) ? color : vec3(0.0);
    fragColor = vec4(color, 1.0);
}