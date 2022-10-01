[BUFFERTYPE:TEXT]
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

uniform float isGradient = 0;
uniform vec4 u_color_a=vec4(1.0,0.0,0.0,1.0);
uniform vec4 u_color_b=vec4(0.0,1.0,1.0,1.0);

layout(location = 0) out vec4 color;
void main(void)
{
    vec4 texColor = texture(textureObject, vec2(texCoord.x+offset.x/u_resolution.x, texCoord.y+1+offset.y/u_resolution.y));
    
    //texColor.rgb= texColor.rgb * u_color.a;
    texColor.a = (texColor.r+texColor.g+texColor.b)/3 * u_color.a;
    
    texColor = texColor * u_color;
    
    if(texColor.a > 0.80){
    texColor.a = 1;
    }
    
    
    texColor.a = pow(texColor.a,14); // make the brights brighter and darks darker
    
    texColor.a = texColor.a*50;
    
    if(texColor.a>1){
    texColor.a = 1;
    }
    
    if (texColor.a < 0.85)
    {
	  discard;
    }
    else
    {
    if(isGradient == 1)
    {
          vec2 uv = (texCoord.xy*10)*vec2(u_resolution.x / u_resolution.y,1.0);

     color = mix(u_color_a,u_color_b, uv.y*10) *texColor.a;
}
else{
	  color = texColor;
	  }
    }
}