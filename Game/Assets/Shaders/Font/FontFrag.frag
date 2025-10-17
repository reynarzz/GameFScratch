#version 330 core

uniform sampler2D uTextures[15]; //uniform sampler2D uTextures[{32}]
in vec2 fragUV;
in vec4 vColor;

flat in int fragTexIndex;
out vec4 fragColor;

void main()
{
    fragColor = texture(uTextures[fragTexIndex], fragUV) * vColor;

    if(fragColor.a <= 0.000001)
    {
        discard;
    }
}