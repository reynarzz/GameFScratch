#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;

flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;

void main()
{
    fragColor = texture(uScreenGrabTex, screenUV) * vColor * vec4(1,1,0,1);
} 