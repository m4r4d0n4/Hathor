#version 330
precision highp float;

uniform sampler2D sampler;

uniform vec3 midPosition;
uniform vec3 camera_position;

uniform float radius;
uniform vec3 color;

uniform sampler2D GNormal;
uniform sampler2D GPosition;
uniform sampler2D GColorAndRoughness;
uniform sampler2D GMetalness;

uniform float screenWidth;
uniform float screenHeight;

const float PI = 3.14159265359;

// output
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
	vec2 fragmentScreenCoordinates = vec2(gl_FragCoord.x / screenWidth, gl_FragCoord.y / screenHeight);
    vec3 fragPos = texture(GPosition, fragmentScreenCoordinates).rgb;
	
	vec4 pCaR = texture(GColorAndRoughness, fragmentScreenCoordinates).rgba;
	vec3 albedo = pCaR.rgb;
	float roughness = pCaR.a;

	float metallic = texture(GMetalness, fragmentScreenCoordinates).r;

	vec3 N = texture(GNormal, fragmentScreenCoordinates).rgb;
	vec3 V = normalize(camera_position - fragPos);

	vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

	vec3 L = normalize(midPosition - fragPos);
    vec3 H = normalize(V + L);
    float distance    = length(midPosition - fragPos);
	float attenuation = 1.0 - distance / radius; 

	vec3 radiance     = color * attenuation;
	float NdotL = dot(N, L);

	float NDF = DistributionGGX(N, H, roughness);        
    float G   = GeometrySmith(N, V, L, roughness);      
    vec3 F    = FresnelSchlickRoughness(max(dot(H, V), 0.0), F0, roughness);               
    vec3 nominator    = NDF * G * F;
    float denominator = 4 * max(dot(N, V), 0.0) * max(NdotL, 0.0) + 0.001; 
    vec3 specular     = nominator / denominator;
    
	outputColor = vec4(max(radiance * specular, vec3(0.0)), 1.0);
}