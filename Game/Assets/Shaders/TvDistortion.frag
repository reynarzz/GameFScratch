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
uniform float uDistortionStrength = 0.05;   // how strong the curvature is
uniform vec3 uBackgroundColor = vec3(0.0); // background color for edges
uniform float uEdgeSoftness = 0.001;        // smooth transition width

void main()
{
    // Normalize coordinates from -1..1 centered at screen center
    vec2 uv = screenUV * 2.0 - 1.0;

    // Apply barrel (convex) distortion
    vec2 distortedUV = uv + uv * (length(uv) * uDistortionStrength);

    // Transform back to 0..1 UVs
    distortedUV = distortedUV * 0.5 + 0.5;

    // Compute distance outside [0,1] for soft edge
    float blendX = smoothstep(0.0, uEdgeSoftness, distortedUV.x) * (1.0 - smoothstep(1.0 - uEdgeSoftness, 1.0, distortedUV.x));
    float blendY = smoothstep(0.0, uEdgeSoftness, distortedUV.y) * (1.0 - smoothstep(1.0 - uEdgeSoftness, 1.0, distortedUV.y));
    float blend = blendX * blendY;

    // Sample screen texture
    vec3 color = texture(uScreenGrabTex, clamp(distortedUV, 0.0, 1.0)).rgb;

    // Mix with background color smoothly
    color = mix(uBackgroundColor, color, blend);

    fragColor = vec4(color, 1.0) * vColor;
}
