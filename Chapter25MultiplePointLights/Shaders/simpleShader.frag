#version 450

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec3 fragPosWorld;
layout(location = 2) in vec3 fragNormalWorld;

layout(location = 0) out vec4 outColor;


struct PointLight {
	vec4 position;  // ignore w
	vec4 color;		// w is intensity
};

layout(set = 0, binding = 0) uniform GlobalUbo
{
	mat4 projection;
	mat4 view;
	vec4 ambientColor;
	PointLight pointLight1;
	int numLights;
} ubo;

layout(push_constant) uniform Push 
{
	mat4 modelMatrix;
	mat4 normalMatrix;
} push;



void main() {
	vec3 diffuseLight = ubo.ambientColor.xyz * ubo.ambientColor.w;
	vec3 surfaceNormal = normalize(fragNormalWorld);
	
	for (int i = 0; i < ubo.numLights; i++)
	{
//		PointLight light = ubo.pointLight1;
//		if (i == 1)
//		{
//			light = ubo.pointLight2;
//		}
//		
		PointLight light = ubo.pointLight1;
		vec3 directionToLight = light.position.xyz - fragPosWorld;
		float attenuation = 1.0 / dot(directionToLight, directionToLight); // distance squared
		float cosAngIncidence = max(dot(surfaceNormal, normalize(directionToLight)), 0);
		vec3 intensity = light.color.xyz * light.color.w * attenuation;

		diffuseLight += intensity * cosAngIncidence;
	}
	
	
    outColor = vec4(diffuseLight * fragColor, 1.0);
}