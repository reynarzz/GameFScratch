#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;

flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

// Amount of wobble distortion
uniform float uDistortionAmount = 0.002;

void main()
{
    // Non-uniform wobbly distortion using multiple frequencies
    float wave1 = sin((screenUV.y * 3.1 + uTime.x * 0.3) * 40.0);
    float wave2 = cos((screenUV.x * 3.1 + uTime.x * 0.2) * 35.0);
    float wave3 = sin((screenUV.x + screenUV.y) * 10.0 + uTime.x * 0.5);

    // Combine waves in a non-uniform way
    vec2 wobble;
    wobble.x = wave1 * uDistortionAmount + wave3 * uDistortionAmount * 0.2;
    wobble.y = wave2 * uDistortionAmount + wave3 * uDistortionAmount * 0.1;

    // Sample scene with distortion
    vec3 base = texture(uScreenGrabTex, screenUV + wobble).rgb;

    fragColor = vec4(base, 1.0) * vColor;
}
