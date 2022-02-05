#version 330 core
precision highp float;

uniform sampler2D gColorRoughness;
uniform sampler2D gNormal;
uniform sampler2D gPosition;
uniform sampler2D gMetalAndShadow;

uniform sampler2D brdfLUT;

uniform samplerCube iblSpecular;
uniform samplerCube iblIrradiance;

uniform vec4 camera_position;

in vec2 texcoord;
out vec4 outputColor;


vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}  

void main()
{             
	vec3 N = texture(gNormal, texcoord).rgb;
	vec4 pos = texture(gPosition, texcoord);
	vec3 V = vec3(normalize(camera_position - pos));
	vec3 R = reflect(-V, N);  
	
	vec4 c = texture(gColorRoughness, texcoord);
	vec3 albedo = c.rgb;
	float roughness = c.a;
	
	vec3 matalAndShadow = texture(gMetalAndShadow, texcoord).rgb;
	float metallic = matalAndShadow.r;

	vec3 F0 = vec3(0.04, 0.04, 0.04); 
    F0 = mix(F0, albedo, metallic);

	vec3 F = FresnelSchlickRoughness(max(dot(N, V), 0.0), F0, roughness);

	vec3 kS = F;
	vec3 kD = 1.0 - kS;
	kD *= 1.0 - metallic;	  
  
	vec3 irradiance = texture(iblIrradiance, N).rgb;
	vec3 diffuse    = irradiance * albedo;
  
	const float MAX_REFLECTION_LOD = 8.0;
	vec3 prefilteredColor = pow(textureLod(iblSpecular, R, roughness * MAX_REFLECTION_LOD).rgb, vec3(1.01));

	vec2 envBRDF  = texture(brdfLUT, vec2(max(dot(N, V), 0.0), roughness)).rg;
	vec3 specular = prefilteredColor * (F * envBRDF.x + envBRDF.y);
  
	vec3 ambient = (kD * diffuse + specular) * matalAndShadow.g;

	outputColor = vec4(ambient + diffuse + specular, 1);
}