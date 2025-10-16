#version 330 core
in vec2 screenUV;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform sampler2D uBlurTex;
uniform float uBloomStrength = 0.5;

void main()
{
    vec3 scene = texture(uScreenGrabTex, screenUV).rgb;
    vec3 bloom = texture(uBlurTex, screenUV).rgb;
    //fragColor = vec4(scene + bloom * uBloomStrength, 1.0);

    vec3 result = scene + bloom * uBloomStrength;
    fragColor = vec4(clamp(result, 0.0, 1.0), 1.0);
}