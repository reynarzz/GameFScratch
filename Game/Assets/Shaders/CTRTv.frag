#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;
flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

// CRT TV Controls
uniform vec3 uBackgroundColor = vec3(0.07);     // background color (edges)
uniform float uDistortionStrength = 0.08;       // barrel distortion
uniform float uEdgeSoftness = 0.001;            // smooth edges
uniform float uScanlineIntensity = 0.45;        // darkness of scanlines
uniform float uScanlineSpacing = 4.0;           // spacing between scanlines
uniform float uPhosphorGlow = 0.4;              // glow on neighboring lines
uniform float uRGBOffset = 1.0;                 // horizontal RGB offset
uniform float uBrightness = 1.0;                // overall brightness
uniform float uContrast = 1.0;                  // overall contrast
uniform vec3 uRGBBalance = vec3(1.0, 0.8, 0.6);// scale of R,G,B channels

void main()
{
    // Normalize coordinates to -1..1 centered
    vec2 uv = screenUV * 2.0 - 1.0;

    // Barrel distortion for sampling
    vec2 distortedUV = uv + uv * (length(uv) * uDistortionStrength);
    distortedUV = distortedUV * 0.5 + 0.5;

    // Smooth edges
    float blendX = smoothstep(0.0, uEdgeSoftness, distortedUV.x) *
                   (1.0 - smoothstep(1.0 - uEdgeSoftness, 1.0, distortedUV.x));
    float blendY = smoothstep(0.0, uEdgeSoftness, distortedUV.y) *
                   (1.0 - smoothstep(1.0 - uEdgeSoftness, 1.0, distortedUV.y));
    float edgeBlend = blendX * blendY;

    vec2 texel = 1.0 / uScreenSize;

    // Sample RGB channels with optional offset
    vec3 color;
    color.r = texture(uScreenGrabTex, clamp(distortedUV + vec2(uRGBOffset * texel.x, 0.0), 0.0, 1.0)).r;
    color.g = texture(uScreenGrabTex, clamp(distortedUV, 0.0, 1.0)).g;
    color.b = texture(uScreenGrabTex, clamp(distortedUV - vec2(uRGBOffset * texel.x, 0.0), 0.0, 1.0)).b;

    // Scanlines in screen space
    vec2 pixelPos = screenUV * uScreenSize;
    float line = mod(pixelPos.y, uScanlineSpacing);
    float scanlineFactor = 1.0 - smoothstep(0.0, 1.0, line) * uScanlineIntensity;
    color *= scanlineFactor;

    // Symmetric phosphor glow (all channels)
    vec3 glowAbove = texture(uScreenGrabTex, clamp(distortedUV + vec2(0.0, texel.y), 0.0, 1.0)).rgb * uPhosphorGlow;
    vec3 glowBelow = texture(uScreenGrabTex, clamp(distortedUV - vec2(0.0, texel.y), 0.0, 1.0)).rgb * uPhosphorGlow;
    color += (glowAbove + glowBelow) * 0.5;

    // Apply RGB balance
    color *= uRGBBalance;

    // Mix with background color for edges
    color = mix(uBackgroundColor, color, edgeBlend);

    // Apply brightness and contrast
    color = (color - 0.5) * uContrast + 0.5;
    color *= uBrightness;

    fragColor = vec4(color, 1.0) * vColor;
}
