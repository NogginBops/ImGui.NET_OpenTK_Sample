[BUFFERTYPE:SPRITE]
[VERTEX]
#version 410 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec4 aTexCoord;

layout(location = 2) in vec4 batching_position;
layout(location = 3) in vec4 batching_size;
layout(location = 4) in vec4 batching_color;
layout(location = 5) in vec4 batching_offset;

out vec4 c;
out vec4 offset;
out vec4 size;
out vec4 texCoord;
out vec4 frag_color;
uniform mat4 u_mvp = mat4(1.0);

void main(void)
{
   c = batching_color;
offset = batching_offset;
size = batching_size;

    texCoord = aTexCoord;

    gl_Position = u_mvp * position;
}

[FRAGMENT]
#version 410 core
in vec4 c;
in vec4 offset;
in vec4 size;
in vec4 texCoord;
uniform sampler2D textureObject;
uniform vec4 u_color;
uniform vec2 u_resolution;
layout(location = 0) out vec4 color;

void main(void)
{
    vec2 pixelSize = vec2(1.0 / u_resolution.x, 1.0 / u_resolution.y);
    vec4 texColor = texture(textureObject, vec2(texCoord.x + pixelSize.x * offset.x, texCoord.y + pixelSize.y * offset.y) * (pixelSize * size.xy+offset.xy)) * u_color*c;
    if (texColor.a < 0.1)
    {
	  discard;
    }
    else
    {
	  color = texColor;
    }
}