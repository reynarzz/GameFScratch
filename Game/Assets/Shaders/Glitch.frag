#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;
flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

// Parameters
uniform float uSplitAmount = 0.005;
uniform float uGlitchSpeed = 5.0;

void main()
{
    // Time-varying glitch offset
    float glitch = sin(uTime.x * uGlitchSpeed + screenUV.y * 10.0) * uSplitAmount;

    vec3 colR = texture(uScreenGrabTex, screenUV + vec2(glitch, 0.0)).rgb;
    vec3 colG = texture(uScreenGrabTex, screenUV).rgb;
    vec3 colB = texture(uScreenGrabTex, screenUV - vec2(glitch, 0.0)).rgb;

    fragColor = vec4(colR.r, colG.g, colB.b, 1.0) * vColor;
}
