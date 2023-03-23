#version 450
#extension GL_KHR_vulkan_glsl:enable


const vec2 OFFSETS[6] = vec2[](
	vec2(-1.0, -1.0),
	vec2(-1.0, 1.0),
	vec2(1.0, -1.0),
	vec2(1.0, -1.0),
	vec2(-1.0, 1.0),
	vec2(1.0, 1.0)
);

layout (location = 0) out vec2 fragOffset;

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
	vec4 position;
	vec4 color;
	float radius;
} push;


//const float LIGHT_RADIUS = 0.06;

void main() {
	fragOffset = OFFSETS[gl_VertexIndex];
	vec3 cameraRightWorld = {ubo.view[0][0], ubo.view[1][0], ubo.view[2][0]};
	vec3 cameraUpWorld = {ubo.view[0][1], ubo.view[1][1], ubo.view[2][1]};
	
	vec3 positionWorld = push.position.xyz
		+ push.radius * fragOffset.x * cameraRightWorld
		+ push.radius * fragOffset.y * cameraUpWorld;
	
	gl_Position = ubo.projection * ubo.view * vec4(positionWorld, 1.0);
}

