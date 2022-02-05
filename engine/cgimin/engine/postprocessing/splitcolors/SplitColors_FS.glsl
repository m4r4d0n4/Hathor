#version 330
precision highp float;

uniform sampler2D sampler;
uniform float threshold;

in vec2 texcoord;

// output
layout (location = 0) out vec4 outputColor;
layout (location = 1) out vec4 brightColor;

void main()
{    
    vec4 color = texture(sampler, texcoord);
    outputColor = color;

    float brightness = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
    if(brightness > threshold)
        brightColor = vec4(color.rgb, 1.0);
	else
		brightColor = vec4(0.0, 0.0, 0.0, 1.0);
    
}