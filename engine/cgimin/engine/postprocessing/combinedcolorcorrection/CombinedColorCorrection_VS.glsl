#version 150
precision highp float;

// input from VAO-Data structure
in vec3 in_position;
in vec3 in_normal; 
in vec2 in_uv; 

out vec2 texcoord;

void main()
{
	texcoord = in_uv;

	gl_Position = vec4(in_position, 1);
}


