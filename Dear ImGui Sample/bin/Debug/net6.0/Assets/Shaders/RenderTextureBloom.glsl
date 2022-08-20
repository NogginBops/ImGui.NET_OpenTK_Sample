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

vec4 getBloom(int radius, float spacing)
{
    vec4 bloom = vec4(0, 0, 0, 0);

    for (int x = -radius; x < radius; x++)
    {
	  for (int y = -radius; y < radius; y++)
	  {
		vec4 pixelColor = texture(textureObject, vec2(texCoord.x + (1.0 / u_resolution.x) * x * spacing, texCoord.y + (1.0 / u_resolution.y) * y * spacing));

		float distance = distance(vec2(texCoord.x, texCoord.y), vec2(texCoord.x + (1.0 / u_resolution.x) * x * spacing, texCoord.y + (1.0 / u_resolution.y) * y * spacing));

		if (pixelColor.x > 0.2 || pixelColor.y > 0.2 || pixelColor.z > 0.2 && pixelColor.w > 0)
		{
		    bloom = vec4(bloom.rgb + pixelColor.rgb, bloom.a + length(pixelColor) * 0.00015);
		}
	  }
    }
    return bloom;
}

void main(void)
{
    vec4 texColor = texture(textureObject, vec2(texCoord.x, texCoord.y));

    // bloom
    vec4 bloomColor = vec4(0, 0, 0, 0);


    //bloomColor += getBloom(10, 1);
    bloomColor += getBloom(20, 2);
    //bloomColor += getBloom(30, 4);

    if (bloomColor.a > 0)
    {
	  texColor = vec4(normalize(bloomColor).rgb * 2, 1) * bloomColor.a;
	  texColor.a = bloomColor.a * 5;
	  color = texColor;
    }

}