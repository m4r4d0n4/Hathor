#version 130
precision highp float;

uniform sampler2D color_texture; 
uniform vec4 color;

in vec2 texcoord;

out vec4 outputColor;

void main()
{
	outputColor = texture(color_texture, texcoord) * color;
}