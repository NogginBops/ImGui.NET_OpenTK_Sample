[BUFFERTYPE:BOX]
[VERTEX]
#version 410 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec4 aTexCoord;

uniform mat4 u_mvp = mat4(1.0);

out vec4 texCoord;
out vec4 frag_color;
uniform vec4 u_color;
uniform vec4 u_tint = vec4(1.0); // tint is for shader editing

void main(void)
{
    texCoord = aTexCoord;

    gl_Position = u_mvp * position;
   // frag_color = vec4( u_color.r * u_tint.r,u_color.g * u_tint.g,u_color.b*u_tint.b, u_color.a*u_tint.a);
}

[FRAGMENT]
#version 410 core
in vec4 texCoord;
uniform vec2 u_resolution;

uniform vec4 u_color_a=vec4(1.0,1.0,0.0,1.0);
uniform vec4 u_color_b=vec4(1.0,0.0,0.0,1.0);

in vec4 frag_color;
out vec4 color;

void main(void)
{
      vec2 uv = (texCoord.xy)*vec2(u_resolution.x / u_resolution.y,1.0);
    

vec4 col = mix(u_color_b,u_color_a, uv.y);
//col+= mix(c1,c2, uv.y);

    color = col;
}