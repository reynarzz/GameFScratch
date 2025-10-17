#version 330 core
layout(location = 0) in vec2 position;
layout(location = 1) in vec2 uv;  
layout(location = 2) in uint color;
layout(location = 3) in int vertIndex;

out vec2 fragUV;
out vec4 vColor;

uniform mat4 uVP;
uniform vec3 uTime;

vec4 unpackColor(uint c) 
{
    float r = float((c >> 24) & 0xFFu) / 255.0;
    float g = float((c >> 16) & 0xFFu) / 255.0;
    float b = float((c >>  8) & 0xFFu) / 255.0;
    float a = float( c        & 0xFFu) / 255.0;
    return vec4(r, g, b, a);
}

vec2 animateCharacter(vec2 pos, int vertIndex, float time)
{
    int charIndex = vertIndex / 4;

    float amplitude = 2.0;   
    float frequency = 4.0;   
    float charOffset = 0.6;  

    float phase = float(charIndex) * charOffset;
    float yOffset = sin(time * frequency + phase) * amplitude;

    return pos + vec2(0.0, yOffset);
}

void main() 
{
    fragUV = uv;
    vColor = unpackColor(color);

    vec2 animatedPos = animateCharacter(position, vertIndex, uTime.y);

    gl_Position = uVP * vec4(animatedPos, 0.0, 1.0);
}
