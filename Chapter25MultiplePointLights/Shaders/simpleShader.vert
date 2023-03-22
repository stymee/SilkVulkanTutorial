#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 color;
layout(location = 2) in vec3 normal;
layout(location = 3) in vec2 uv;

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec3 fragPosWorld;
layout(location = 2) out vec3 fragNormalWorld;

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
	PointLight pointLight2;
	PointLight pointLight3;
	PointLight pointLight4;
	PointLight pointLight5;
	PointLight pointLight6;
	int numLights;
} ubo;

layout(push_constant) uniform Push 
{
	mat4 modelMatrix;
	mat4 normalMatrix;
} push;


//const vec3 DIRECTION_TO_LIGHT = normalize(vec3(1.0, -3.0, -1.0));
//const float AMBIENT = 0.02;

void main() {
	vec4 positionWorld = push.modelMatrix * vec4(position, 1.0);
	gl_Position = ubo.projection * ubo.view * positionWorld;
	
	fragNormalWorld = normalize(mat3(push.normalMatrix) * normal);
	fragPosWorld = positionWorld.xyz;
	fragColor = color;

//	vec3 directionToLight = ubo.lightPosition.xyz - positionWorld.xyz;
//	float attenuation = 1.0 / dot(directionToLight, directionToLight); // distance squared
//	
//	vec3 lightColor = ubo.lightColor.xyz * ubo.lightColor.w * attenuation;
//	vec3 ambientLight = ubo.ambientColor.xyz * ubo.ambientColor.w;
//	vec3 diffuseLight = lightColor * max(dot(normalWorldSpace, normalize(directionToLight)), 0);
//
//	fragColor = (diffuseLight + ambientLight) * color;
}


//#extension GL_KHR_vulkan_glsl:enable