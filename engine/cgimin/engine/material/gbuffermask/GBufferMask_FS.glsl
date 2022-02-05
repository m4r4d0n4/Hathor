#version 330
precision highp float;

uniform sampler2D color_texture; 
uniform sampler2D normalmap_texture;
uniform sampler2D maskmap_texture;

uniform float emission;

in vec2 texcoord;
in vec3 position;
in mat3 fragTBN;

// output
layout (location = 0) out vec4 gColorRoughness;
layout (location = 1) out vec3 gPosition;
layout (location = 2) out vec3 gNormal;
layout (location = 3) out vec3 gMetalnessShadow;
layout (location = 4) out vec3 gGlow;

void main()
{
	vec4 mask = texture(maskmap_texture, texcoord);

	gColorRoughness.rgb = texture(color_texture, texcoord).rgb * emission;
	gColorRoughness.a = 1.0 - mask.a;

	gMetalnessShadow.r = mask.r;
	gMetalnessShadow.g = 1.0;

	gPosition = position;
	
	vec3 normal = texture(normalmap_texture, texcoord).rgb;
	normal = normalize(normal * 2.0 - 1.0); 
	normal = normalize(fragTBN * normal); 
	gNormal = normal;
}