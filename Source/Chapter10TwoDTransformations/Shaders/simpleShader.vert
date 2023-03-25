#version 450

layout(location = 0) in vec2 position;
layout(location = 1) in vec3 color;

layout(push_constant) uniform Push 
{
	mat2 transform;
	vec4 offset;
	vec4 color;
} push;


void main() {
	gl_Position = vec4(push.transform * position + push.offset.xy, 0.0, 1.0);
}

//#extension GL_KHR_vulkan_glsl:enable