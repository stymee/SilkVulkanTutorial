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
	vec4 front;
	vec4 ambientColor;
	int numLights;
	int padding1;
	int padding2;
	int padding3;
	PointLight pointLights[10];
//	PointLight pointLight1;
//	PointLight pointLight2;
//	PointLight pointLight3;
//	PointLight pointLight4;
//	PointLight pointLight5;
//	PointLight pointLight6;
} ubo;

layout(push_constant) uniform Push 
{
	mat4 modelMatrix;
	mat4 normalMatrix;
} push;



void main() {
	vec3 diffuseLight = ubo.ambientColor.xyz * ubo.ambientColor.w;
	vec3 specularLight = vec3(0.0);
	vec3 surfaceNormal = normalize(fragNormalWorld);
	
	// already had front vec in camera 
	vec3 viewDirection = -ubo.front.xyz;

	for (int i = 0; i < ubo.numLights; i++)
	{
//		PointLight light = ubo.pointLight1;
//		if (i == 1) light = ubo.pointLight2;
//		else if (i == 2) light = ubo.pointLight3;
//		else if (i == 3) light = ubo.pointLight4;
//		else if (i == 4) light = ubo.pointLight5;
//		else if (i == 5) light = ubo.pointLight6;
		
		PointLight light = ubo.pointLights[i];
		vec3 directionToLight = light.position.xyz - fragPosWorld;
		float attenuation = 1.0 / dot(directionToLight, directionToLight); // distance squared
		directionToLight = normalize(directionToLight);

		float cosAngIncidence = max(dot(surfaceNormal, directionToLight), 0);
		vec3 intensity = light.color.xyz * light.color.w * attenuation;

		diffuseLight += intensity * cosAngIncidence;

		// specular lighting
		vec3 halfAngle = normalize(directionToLight + viewDirection);
		float blinnTerm = dot(surfaceNormal, halfAngle);
		blinnTerm = clamp(blinnTerm, 0, 1);
		blinnTerm = pow(blinnTerm, 64.0);
		specularLight += intensity * blinnTerm;
	}
	
	
    outColor = vec4(diffuseLight * fragColor + specularLight * fragColor, 1.0);
}