#version 450
#extension GL_KHR_vulkan_glsl:enable

layout(location = 0) in vec2 position;

void main() {
	gl_Position = vec4(position, 0.0, 1.0);
}
