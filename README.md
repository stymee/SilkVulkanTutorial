# C# Silk.NET Vulkan Game Engine Tutorial

![screenshot of app](https://github.com/stymee/SilkVulkanTutorial/blob/master/Docs/screenshot1.png?raw=true)
https://www.youtube.com/watch?v=GIFfxnux9mo

- Based on the excellent Youtube tutorial by Brendan Galea - Vulkan Game Engine Tutorial (C++)
	- https://www.youtube.com/playlist?list=PL8327DO66nu9qYVKLDmdLW_84-yE4auCR

- Credit also to C# Silk.NET version of the OG Vulkan C++ Tutorial by Daniel Keenan
	- https://github.com/dfkeenan/SilkVulkanTutorial
	- https://vulkan-tutorial.com/

- And Mellinoe for the ImGui Vulkan example in the main Silk.NET repo
	- https://github.com/dotnet/Silk.NET/tree/main/src/Lab/Experiments/ImGuiVulkan

- And the entire Silk.NET team and discord 

## TO DO:
- Docs:
	- Add some notes for each chapter describing where I deviated from the C++ (and maybe why)
	- Maybe add some some youtube tutorials for the C# stuff?

- Build:
	- Fix up the shader compiler build step to only compile if glsl files have changed
	- Get the shader compiler path to work with an env variable (I'm dense)

- Features:
	- Add some new tutorial chapters (textures?, what else?)
	- Points and lines rendering
	- Perspective camera currently doesn't work
	

## Build requirements !!! VERY IMPORTANT !!!

The projects require the [Vulkan SDK](https://www.lunarg.com/vulkan-sdk/) to build/run. The SDK provides the Vulkan validation layers as well as the command line tools to compile the shaders. 

You will have to update your csproj files to point the folder where glslc.exe is found (this is the glsl compiler)

```xml
	<PropertyGroup>
		<VulkanBinPath>C:\VulkanSDK\1.3.239.0\Bin</VulkanBinPath>
	</PropertyGroup>
```
![screenshot of vulkan path](https://github.com/stymee/SilkVulkanTutorial/blob/master/Docs/screenshot3.png?raw=true)

** I couldn't get it to work with an environment variable, and I'm sure there's a better way to handle this globally


## Each chapter was re-coded in C# (.NET 7)
## Two bonus projects that include MultiSampling and ImGui integration
![screenshot of solution](https://github.com/stymee/SilkVulkanTutorial/blob/master/Docs/screenshot2.png?raw=true)
