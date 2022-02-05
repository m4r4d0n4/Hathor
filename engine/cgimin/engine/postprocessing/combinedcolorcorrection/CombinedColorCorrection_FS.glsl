#version 330
precision highp float;

uniform sampler2D normalColor;
uniform sampler2D highColor;

uniform float exposure;
uniform float gamma;

in vec2 texcoord;

out vec4 outputColor;

void main()
{    
    vec3 hdrColor = texture(normalColor, texcoord).rgb;      
    vec3 bloomColor = texture(highColor, texcoord).rgb;

    hdrColor += bloomColor;

    vec3 result = vec3(1.0) - exp(-hdrColor * exposure);  
    result = pow(result, vec3(1.0 / gamma));

    outputColor = vec4(result, 1.0);
    
}