#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 color;
layout(location = 2) in vec3 normal;
layout(location = 3) in vec2 uv;

layout(location = 0) out vec3 fragColor;

layout(set = 0, binding = 0) uniform GlobalUbo
{
	mat4 projectionViewMatrix;
	vec4 directionToLight;
} ubo;

layout(push_constant) uniform Push 
{
	mat4 modelMatrix;
	mat4 normalMatrix;
} push;


//const vec3 DIRECTION_TO_LIGHT = normalize(vec3(1.0, -3.0, -1.0));
const float AMBIENT = 0.02;

void main() {
	gl_Position = ubo.projectionViewMatrix * push.modelMatrix * vec4(position.xyz, 1.0);

	// temporary: this is only correct in certain situations!
	// only works correctly if scale is uniform!
	//vec3 normalWorldSpace = normalize(mat3(push.modelMatrix) * normal);

	// use this to get the right result, but then this is more computationaly expensive
	//mat3 normalMatrix = transpose(inverse(mat3(push.modelMatrix)));
	//vec3 normalWorldSpace = normalize(normalMatrix * normal);


	// precompute normalMatrix and send it in with the attributes
	vec3 normalWorldSpace = normalize(mat3(push.normalMatrix) * normal);

	float lightIntensity = AMBIENT + max(dot(normalWorldSpace, ubo.directionToLight.xyz), 0);

	fragColor = lightIntensity * color;
}

//#extension GL_KHR_vulkan_glsl:enable