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
uniform float uScanlineIntensity = 0.25;     // darkness of scanlines
uniform float uScanlineSpacing = 5.0;        // spacing between scanlines in pixels
uniform float uPhosphorGlow = 0.3;           // phosphor glow
uniform float uRGBOffset = 1.0;              // RGB horizontal offset
uniform vec3 uBackgroundColor = vec3(0.0);   // background color (default black)

void main()
{
    // Calculate screen coordinates in pixels
    vec2 pixelPos = screenUV * uScreenSize;

    // Compute scanline factor (0 = dark line, 1 = bright line)
    float line = mod(pixelPos.y, uScanlineSpacing);
    float scanlineFactor = 1.0 - smoothstep(0.0, 1.0, line) * uScanlineIntensity;

    // Sample the original screen
    vec2 texel = 1.0 / uScreenSize;

    // Optional RGB offset for chromatic feel
    vec3 texColor;
    texColor.r = texture(uScreenGrabTex, screenUV + vec2(uRGBOffset * texel.x, 0.0)).r;
    texColor.g = texture(uScreenGrabTex, screenUV).g;
    texColor.b = texture(uScreenGrabTex, screenUV - vec2(uRGBOffset * texel.x, 0.0)).b;

    // Apply scanline darkness
    texColor *= scanlineFactor;

    // Optional phosphor glow to neighboring lines
    float glowAbove = texture(uScreenGrabTex, screenUV + vec2(0.0, texel.y)).r * uPhosphorGlow;
    float glowBelow = texture(uScreenGrabTex, screenUV - vec2(0.0, texel.y)).r * uPhosphorGlow;
    texColor.r += glowAbove;
    texColor.b += glowBelow;

    // Mix with background color to control overall tint
    vec3 finalColor = mix(uBackgroundColor, texColor, texColor.r + texColor.g + texColor.b > 0.0 ? 1.0 : 0.0);

    fragColor = vec4(finalColor, 1.0) * vColor;
}
