/*
 * Based on - Vulkan Example - Using mesh shaders
 *
 * Copyright (C) 2022 by Sascha Willems - www.saschawillems.de
 *
 * This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
 */

#version 460

layout (location = 0) in VertexInput {
  vec4 color;
} vertexInput;

layout(location = 0) out vec4 outFragColor;
 

void main()
{
	outFragColor = vertexInput.color;
}

//layout(location = 0) in vec3 fragColor;
//
//layout(location = 0) out vec4 outColor;
//
//layout(push_constant) uniform Push 
//{
//	mat4 modelMatrix;
//} push;
//
//
//void main() {
//    outColor = vec4(fragColor, 1.0);
//}