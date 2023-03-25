# C# Silk.NET Vulkan Game Engine Tutorial

![](../Docs/screenshot1.png)
https://www.youtube.com/watch?v=GIFfxnux9mo

- Based on the excellent Youtube tutorial by Brendan Galea - Vulkan Game Engine Tutorial (C++)
	- https://www.youtube.com/playlist?list=PL8327DO66nu9qYVKLDmdLW_84-yE4auCR

- Credit also to C# Silk.NET version of the OG Vulkan C++ Tutorial by Daniel Keenan
	- https://github.com/dfkeenan/SilkVulkanTutorial
	- https://vulkan-tutorial.com/

- And Mellinoe for the ImGui Vulkan example in the main Silk.NET repo
	- https://github.com/dotnet/Silk.NET/tree/main/src/Lab/Experiments/ImGuiVulkan

- And the entire Silk.NET team and discord 
	

## Build requirements

The projects require the [Vulkan SDK](https://www.lunarg.com/vulkan-sdk/) to build/run. The SDK provides the Vulkan validation layers as well as the command line tools to compile the shaders. You may have to point the csproj to your Vulkan SDK path (I couldn't get it to work with an environment variable)


## Each chapter was re-coded in C# (.NET 7)
## Two bonus projects that include MultiSampling and ImGui integration
