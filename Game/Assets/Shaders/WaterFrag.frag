#version 330 core

in vec2 fragUV;
out vec4 fragColor;

uniform vec3  uTime;                     
uniform float uWaveAmplitude  = 0.08;    
uniform float uWaveFrequency  = 8.0;     
uniform float uWaveSpeed      = 3.0;     
uniform float uNoiseScale     = 15.0;    
uniform float uNoiseStrength  = 0.15;    
uniform vec3  uWaterColor     = vec3(0.2, 0.2, 1.0);
uniform float uOutlineHeight  = 0.85;     
uniform vec3  uOutlineColor   = vec3(1.0, 1.0, 1.0);
uniform float uOutlineThickness = 0.04;  

// Stable hash function — avoids NaN / denormal issues
float hash(float n) {
    return fract(fract(n * 0.1031) * 43758.5453123);
}

// Stable 1D noise
float noise(float x) {
    float i = floor(x);
    float f = fract(x);
    float u = f * f * (3.0 - 2.0 * f);
    return mix(hash(i), hash(i + 1.0), u);
}

void main()
{
    float randomAmp = uWaveAmplitude * mix(0.5, 1.5, noise(fragUV.x * uNoiseScale));

    float wave = sin(fragUV.x * uWaveFrequency + uTime.x * uWaveSpeed)
               * randomAmp;

    float wobble = (noise(fragUV.x * uNoiseScale * 0.3 + uTime.x * 0.5) - 0.5)
                 * uNoiseStrength;

    float topEdge = uOutlineHeight + wave + wobble;

    if (fragUV.y > topEdge)
        discard;

    float distanceToEdge = abs(fragUV.y - topEdge);
    float outlineFactor = step(uOutlineThickness, distanceToEdge);

    vec3 color = mix(uOutlineColor, uWaterColor, outlineFactor);
    fragColor = vec4(color, 1.0);
}
