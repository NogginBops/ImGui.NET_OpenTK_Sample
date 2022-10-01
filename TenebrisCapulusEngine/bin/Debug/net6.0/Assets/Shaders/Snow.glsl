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
uniform vec2 u_resolution=vec2(1000,500);
uniform float time = 0;

float snow(vec2 uv, float scale) {
    float w = smoothstep(1.0, 0.0, -uv.y * (scale / 10.0));
    
    if (w < 0.1) {
      return 0.0;
    }
    
    float c = time / scale;
    
    // Fall to left:
    // uv += c;
    
    uv.y += c;
    uv.x -= c;

    uv.y += c * 2.0;
    uv.x += cos(uv.y + time * 0.5) / scale;
    uv   *= scale;

    vec2 s = floor(uv);
    vec2 f = fract(uv);
    vec2 p = vec2(0.0);

    float k = 3.0;
    float d = 0.0;
    
    p = 0.5 + 0.35 * sin(11.0 * fract(sin((s + p + scale) * mat2(7, 3, 6, 5)) * 5.0)) - f;
    d = length(p);
    k = min(d, k);

    k = smoothstep(0.0, k, sin(f.x + f.y) * 0.01);
    return k * w;
  }

  void main (void) {
    vec2 uv = (texCoord.xy)*vec2(u_resolution.x / u_resolution.y,1.0);
    float c = smoothstep(1.0, 0.0, clamp(uv.y * 0.1 + 0.75, 0.0, 0.75));

    c += snow(uv, 30.0) * 0.3;
    c += snow(uv, 20.0) * 0.5;
    c += snow(uv, 15.0) * 0.8;

    c += snow(uv, 10.0);
    c += snow(uv, 8.0);
    c += snow(uv, 6.0);
    c += snow(uv, 5.0);

    color = vec4(vec3(c), 1) * vec4(0.7f,0.8f,0.9f,1); // 0.0
  }