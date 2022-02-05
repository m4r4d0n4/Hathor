#version 130
precision highp float;

uniform sampler2D sampler; 
uniform float alpha;


in vec2 texcoord;

out vec4 outputColor;

void main()
{
    outputColor = texture(sampler, texcoord);
	outputColor.a *= alpha;
}