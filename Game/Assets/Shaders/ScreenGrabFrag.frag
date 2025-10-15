#version 330 core

in vec2 fragUV;
in vec2 screenUV;
in vec4 vColor;

flat in int fragTexIndex;
out vec4 fragColor;

uniform sampler2D uScreenGrabTex;
uniform vec2 uScreenSize;
uniform vec3 uTime;

void main()
{
    // Sample the texture
    vec4 color = texture(uScreenGrabTex, screenUV) * vColor;

    // Convert to grayscale using luminance
    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));

    fragColor = vec4(vec3(gray), color.a);
}
