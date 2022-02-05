#version 330
precision highp float;

uniform sampler2D color_texture; 
uniform sampler2D normalmap_texture;
uniform sampler2D maskmap_texture;

uniform float emission;

uniform float dist3;
uniform float dist2;
uniform float dist1;

uniform sampler2DShadow shadowmap_texture1;
uniform sampler2DShadow shadowmap_texture2;
uniform sampler2DShadow shadowmap_texture3;

in vec2 texcoord;
in vec3 position;
in mat3 fragTBN;

in vec4 viewPosition;
in vec4 ShadowCoord[3];

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

	float visibility = 1.0;
	if (-viewPosition.z < dist1) {
		visibility = texture(shadowmap_texture1, vec3(ShadowCoord[0].xy, ShadowCoord[0].z/ShadowCoord[0].w * 0.998));
	} else if (-viewPosition.z < dist2) {
		visibility = texture(shadowmap_texture2, vec3(ShadowCoord[1].xy, ShadowCoord[1].z/ShadowCoord[1].w * 0.998));
	} else if (-viewPosition.z < dist3) {
		visibility = texture(shadowmap_texture3, vec3(ShadowCoord[2].xy, ShadowCoord[2].z/ShadowCoord[2].w * 0.998));
	}

	gMetalnessShadow.g = visibility;
	gMetalnessShadow.r = mask.r;

	gPosition = position;
	
	vec3 normal = texture(normalmap_texture, texcoord).rgb;
	normal = normalize(normal * 2.0 - 1.0); 
	normal = normalize(fragTBN * normal); 
	gNormal = normal;
}