#version 130
precision highp float;

// input aus der VAO-Datenstruktur
in vec3 in_position;
in vec2 in_uv; 

uniform vec3 char_position;
uniform mat4 projection_matrix;

// "texcoord" wird an den Fragment-Shader weitergegeben, daher als "out" deklariert
out vec2 texcoord;

void main()
{
	// "in_uv" (Texturkoordinate) wird direkt an den Fragment-Shader weitergereicht
	texcoord = in_uv;

	gl_Position = projection_matrix * vec4(in_position + char_position, 1);
}


