[BUFFERTYPE:MODEL]
[VERTEX]
#version 410 core

layout(location = 0) in vec3 a_pos;
layout(location = 1) in vec2 a_texCoord;
out vec2 texCoord;

uniform mat4 u_mvp = mat4(1.0);


void main(void)
 {
  gl_Position = u_mvp * vec4(a_pos.xyz, 1.0);
  texCoord = a_texCoord;

}

[FRAGMENT]
#version 410 core
uniform vec4 u_rendererColor;
out vec4 frag_color;
in vec2 texCoord;
uniform sampler2D textureObject;

void main(void)
{
    vec4 texColor = texture(textureObject,texCoord);
    if (texColor.a < 0.1)
    {
	  discard;
    }
    else
    {
	  frag_color = texColor * u_rendererColor;
    }
}