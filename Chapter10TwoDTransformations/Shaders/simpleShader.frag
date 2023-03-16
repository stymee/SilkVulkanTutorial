#version 450

layout(location = 0) out vec4 outColor;

layout(push_constant) uniform Push 
{
	mat2 transform;
	vec4 offset;
	vec4 color;
} push;



void main() {
    outColor = vec4(push.color.xyz, 1.0);
}