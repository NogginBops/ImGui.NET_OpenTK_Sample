[BUFFERTYPE:SPRITE]
[VERTEX]
#version 410 core

layout(location = 0) in vec4 position;

layout(location = 1) in vec4 aTexCoord;

layout(location = 2) in vec4 batching_position;
layout(location = 3) in vec4 batching_size;
layout(location = 4) in vec4 batching_color;

out vec4 c;
out vec4 texCoord;
out vec4 frag_color;
uniform mat4 u_mvp = mat4(1.0);
uniform mat4 u_unitScaleMatrix = mat4(1.0);

void main(void)
{
   c = batching_color;
    texCoord = aTexCoord;
    vec4 temp =vec4( position.x* batching_size.x  +batching_position.x ,position.y*batching_size.y+batching_position.y,position.z,position.w);
    gl_Position =  (u_mvp * (temp));
}

[FRAGMENT]
#version 410 core
in vec4 c;
in vec4 texCoord;
uniform sampler2D textureObject;
uniform vec4 u_color;
layout(location = 0) out vec4 color;

void main(void)
{
    vec4 texColor = texture(textureObject, vec2(texCoord.x, texCoord.y)) * u_color *  c;
    if (texColor.a < 0.1)
    {
	  discard;
    }
    else
    {
	  color = texColor;
    }
}