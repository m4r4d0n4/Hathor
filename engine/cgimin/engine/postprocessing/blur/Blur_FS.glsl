#version 330
precision highp float;

uniform sampler2D sampler; 
uniform float screenWidth;
uniform float screenHeight;

uniform vec2 blurDirection;

in vec2 texcoord;
out vec4 outputColor;

uniform float weight[5] = float[] (0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162);

void main()
{
   vec2 resolution = vec2(1.0 / screenWidth, 1.0 / screenHeight);
   vec3 result = texture(sampler, texcoord).rgb * weight[0];

   for(int i = 1; i < 5; ++i)
   {
      vec2 lookUp = vec2(resolution.x * i * blurDirection.x, resolution.y * i * blurDirection.y);
      result += texture(sampler, texcoord + lookUp).rgb * weight[i];
      result += texture(sampler, texcoord - lookUp).rgb * weight[i];
   }

   outputColor = vec4(result, 1.0);

}