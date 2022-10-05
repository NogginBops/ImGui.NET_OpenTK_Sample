[BUFFERTYPE:MODEL]
[VERTEX]
#version 410 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec3 vertex_color;

uniform mat4 u_mvp = mat4(1.0);

out vec3 color;

void main(void)
 {
  gl_Position = u_mvp * vec4(pos.xyz, 1.0);
  color = vertex_color;
}

[FRAGMENT]
#version 410 core
in vec3 color;
out vec4 frag_color;

void main(void)
{
    frag_color = vec4(color.xyz, 1.0);
}