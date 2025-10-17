#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;
in vec2 worldUV;

flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;
uniform sampler2D uStarsTex;

// Amount of wobble distortion
uniform float uDistortionAmount = 0.009;

uniform vec3 uOutlineColor = vec3(1.6, 1.0, 1.0);
uniform float uOutlineThickness = 0.02; // fraction of UV
uniform bool uDotted = false;            
uniform float uDotSpacing = 0.15;        // spacing between dots in UV units

// Extra glitch parameters
uniform float uGlitchMaxOffset = 0.03;
uniform float uGlitchFreq = 20.0;
uniform float uColorSplit = 0.001;

float rand(float x) { return fract(sin(x * 1234.5) * 5678.9); }
float rand(vec2 v) { return fract(sin(dot(v, vec2(12.9898,78.233))) * 43758.5453); }

vec3 getQuadEdgeOutline(vec2 uv)
{
    float left   = step(uv.x, uOutlineThickness);
    float right  = step(1.0 - uv.x, uOutlineThickness);
    float bottom = step(uv.y, uOutlineThickness);
    float top    = step(1.0 - uv.y, uOutlineThickness);

    float edge = max(max(left, right), max(top, bottom));

    if (uDotted)
    {
        float pattern = 1.0;
        if (left > 0.5 || right > 0.5) pattern = step(0.5, fract(uv.y / uDotSpacing));
        if (top > 0.5 || bottom > 0.5) pattern = step(0.5, fract(uv.x / uDotSpacing));
        edge *= pattern;
    }

    return uOutlineColor * edge;
}

vec2 getGlitchOffset(vec2 uv, float time)
{
    // Random chance to trigger glitch (tweak probability here)
    float glitchChance = rand(floor(time * 10.0)); // change 10.0 to control frequency
    float glitchPulse = step(0.95, glitchChance);  // 5% chance per "frame"

    // Only apply glitch if glitchPulse > 0
    float rowOffset = sin(uv.y * 40.0 + time * 30.0) * uGlitchMaxOffset * glitchPulse;

    // Vertical jitter
    float vOffset = sin(uv.x * 30.0 + time * 50.0) * uGlitchMaxOffset * glitchPulse * 0.5;

    // Random flicker
    float flicker = step(0.98, rand(floor(uv * 1000.0) + time));
    rowOffset += flicker * uGlitchMaxOffset * 0.5 * glitchPulse;

    return vec2(rowOffset, vOffset);
}

void main()
{
    // Wobbly distortion
    float wave1 = sin((screenUV.y * 3.1 + uTime.x * 0.3) * 40.0);
    float wave2 = cos((screenUV.x * 3.1 + uTime.x * 0.2) * 35.0);
    float wave3 = sin((screenUV.x + screenUV.y) * 10.0 + uTime.x * 0.5);

    vec2 wobble;
    wobble.x = wave1 * uDistortionAmount + wave3 * uDistortionAmount * 0.2;
    wobble.y = wave2 * uDistortionAmount + wave3 * uDistortionAmount * 0.1;

    // Glitch offsets
    vec2 glitch = getGlitchOffset(screenUV, uTime.x);

    // Color channel split
    vec2 rUV = screenUV + wobble + glitch + vec2(uColorSplit, 0.0);
    vec2 gUV = screenUV + wobble + glitch;
    vec2 bUV = screenUV + wobble + glitch - vec2(uColorSplit, 0.0);

    vec3 base = vec3(
        texture(uScreenGrabTex, rUV).r,
        texture(uScreenGrabTex, gUV).g,
        texture(uScreenGrabTex, bUV).b
    );

    vec3 outline = getQuadEdgeOutline(fragUV);
    float aspect= uScreenSize.x / uScreenSize.y;

    vec3 stars = vec3(texture(uStarsTex, vec2(worldUV.x, worldUV.y) * 2.5 + vec2(0, uTime.x * 0.2)).a);

    fragColor = vec4(base + stars + outline, 1.0) * vColor;
}
