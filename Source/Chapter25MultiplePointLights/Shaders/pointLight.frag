#version 450

layout(location = 0) in vec2 fragOffset;
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
	PointLight pointLight2;
	PointLight pointLight3;
	PointLight pointLight4;
	PointLight pointLight5;
	PointLight pointLight6;
	int numLights;
} ubo;

layout(push_constant) uniform Push
{
	vec4 position;
	vec4 color;
	float radius;
} push;

const float M_PI = 3.1415926538;

void main() {
	float dis = sqrt(dot(fragOffset, fragOffset));
	if (dis >= 1.0) discard;
	float dis2 = dis * .9;
	float intensity = 1.0 - pow(dis2, 0.5);
	outColor = vec4(push.color.xyz * intensity, 1.0);
}