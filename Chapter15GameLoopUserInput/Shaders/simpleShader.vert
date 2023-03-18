#version 450

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 color;

layout(location = 0) out vec3 fragColor;

layout(push_constant) uniform Push 
{
	mat4 transform;
	vec4 color;
} push;


void main() {
	//gl_Position = vec4(push.transform * position + push.offset.xy, 0.0, 1.0);
	gl_Position = push.transform * vec4(position.xyz, 1.0);
	fragColor = color;
}

//#extension GL_KHR_vulkan_glsl:enable