[BUFFERTYPE:SPRITE]
[VERTEX]
#version 410 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec4 aTexCoord;

out vec4 texCoord;
out vec4 frag_color;
uniform mat4 u_mvp = mat4(1.0);
uniform vec2 zoomAmount = vec2(1);
uniform mat4 u_unitScaleMatrix = mat4(1.0);

void main(void)
{
    texCoord = aTexCoord/vec4(zoomAmount.x,zoomAmount.y,1,1);
    gl_Position =  (u_mvp * (position));
}

[FRAGMENT]
#version 410 core
in vec4 c;
in vec4 texCoord;
uniform sampler2D textureObject;
uniform vec4 u_color=vec4(1.0);
uniform vec2 u_resolution=vec2(100.0);
uniform vec2 offset =vec2(0,0);
layout(location = 0) out vec4 color;

void main(void)
{
    vec4 texColor = texture(textureObject, vec2(texCoord.x+offset.x/u_resolution.x, texCoord.y+1+offset.y/u_resolution.y)) * u_color;
    if (texColor.a < 0.1)
    {
	  discard;
    }
    else
    {
	  color = texColor;
    }
}