﻿[BUFFERTYPE:SPRITE]
[VERTEX]
#version 410 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec4 aTexCoord;

out vec4 texCoord;
out vec4 frag_color;
uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_unitScaleMatrix = mat4(1.0);

void main(void)
{
    texCoord = aTexCoord;
    gl_Position =  (u_mvp * (position));
}

[FRAGMENT]
#version 410 core
in vec4 c;
in vec4 texCoord;
uniform sampler2D textureObject;
uniform vec4 u_color=vec4(1.0);
layout(location = 0) out vec4 color;

void main(void)
{
    vec4 texColor = texture(textureObject, vec2(texCoord.x+0.5, texCoord.y+0.5)) * u_color;
    if (texColor.a < 0.1)
    {
	  discard;
    }
    else
    {
	  color = texColor;
    }
}