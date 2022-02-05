#version 330
precision highp float;

// input aus der VAO-Datenstruktur
in vec3 in_position;
in vec3 in_normal; 
in vec2 in_uv;
in vec3 in_tangent;
in vec3 in_bitangent;

uniform mat4 modelview_projection_matrix;
uniform mat4 model_matrix;

out vec2 texcoord;
out vec3 position;
out mat3 fragTBN;

void main()
{

	vec3 T = normalize(vec3(model_matrix * vec4(in_tangent, 0.0)));
    vec3 B = normalize(vec3(model_matrix * vec4(in_bitangent, 0.0)));
    vec3 N = normalize(vec3(model_matrix * vec4(in_normal, 0.0)));
    fragTBN = mat3(T, B, N);

	texcoord = in_uv;

	position = vec3(model_matrix * vec4(in_position, 1));

	gl_Position = modelview_projection_matrix * vec4(in_position, 1);
}


