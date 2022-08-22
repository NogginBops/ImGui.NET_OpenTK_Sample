[BUFFERTYPE:RENDERTEXTURE]
[VERTEX]
#version 410 core

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);

out vec2 texCoord;
out vec4 frag_color;

void main(void)
{
    texCoord = aTexCoord;

    gl_Position = u_mvp * vec4(1,1,1,1);
}

[FRAGMENT]
#version 410 core
in vec2 texCoord;
uniform sampler2D textureObject;
layout(location = 0) out vec4 color;
uniform vec2 u_resolution;

void main(void)
{
    vec4 texColor = texture(textureObject,texCoord);
    color = texColor;
}