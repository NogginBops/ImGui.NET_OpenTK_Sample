[BUFFERTYPE:RENDERTEXTURE]
[VERTEX]
#version 410 core

layout(location = 0) in vec4 position;

layout(location = 1) in vec4 aTexCoord;

out vec4 texCoord;
out vec4 frag_color;
uniform mat4 u_mvp = mat4(1.0);
void main(void)
{
    texCoord = aTexCoord;

    gl_Position = u_mvp * position;
}

[FRAGMENT]
#version 410 core
in vec4 texCoord;
uniform sampler2D textureObject;
layout(location = 0) out vec4 color;
uniform vec2 u_resolution = vec2(1000, 500);

void main(void)
{
    vec4 texColor = texture(textureObject, vec2(texCoord.x, texCoord.y));

    vec2 relativePosition = texCoord.xy / vec2(1, 1) - 0.5;
    float len = length(relativePosition);
    float vignette = smoothstep(.9, .2, len);
    texColor.rgb = mix(texColor.rgb, texColor.rgb * vignette, .7);

    texColor.a = 1;

    color = texColor;
}