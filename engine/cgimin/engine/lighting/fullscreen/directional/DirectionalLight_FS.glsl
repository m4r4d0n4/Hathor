#version 330 core
precision highp float;

uniform sampler2D GNormal;
uniform sampler2D GPosition;
uniform sampler2D GColorAndRoughness;
uniform sampler2D GMetalness;

uniform vec3 cameraPosition;
uniform vec3 lightDirection;
uniform vec3 lightColor;

uniform float ambientIntensity;

const float PI = 3.14159265359;

in vec2 texcoord;

out vec4 outputColor;

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float nom   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float nom   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return nom / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

vec3 FresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
} 

void main()
{	
    vec4 pCaR = texture(GColorAndRoughness, texcoord).rgba;
	vec3 albedo = pCaR.rgb;
	float roughness = pCaR.a;

    vec3 radiance = lightColor;

    vec4 matalAndShadow = texture(GMetalness, texcoord);
    float metallic = matalAndShadow.r;
    
    vec3 fragPos = texture(GPosition, texcoord).rgb;

    vec3 N = texture(GNormal, texcoord).rgb;
	vec3 V = normalize(cameraPosition - fragPos);

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    vec3 H = normalize(V + lightDirection);

    float NDF = DistributionGGX(N, H, roughness);        
    float G   = GeometrySmith(N, V, lightDirection, roughness);      
    vec3 F    = FresnelSchlickRoughness(max(dot(H, V), 0.0), F0, roughness);
    //vec3 F    = FresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 nominator    = NDF * G * F;
    float denominator = 4 * max(dot(N, V), 0.0) * max(dot(N, lightDirection), 0.0) + 0.001; 
    vec3 specular     = nominator / denominator;

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;	  

    float NdotL = max(dot(N, lightDirection), 0.0);      

    vec3 Lo = (kD * albedo / PI + specular) * radiance * NdotL;

    vec3 ambient = vec3(ambientIntensity) * albedo;
    vec3 color = ambient + Lo;

    outputColor = vec4(color, 1.0) * matalAndShadow.g;


}