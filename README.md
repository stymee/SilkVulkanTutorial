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

- **And the entire Silk.NET team and discord, y'all are awesome**

- Here are some other resources that helped me tremendously
	- Sascha Willems and his giant collection of examples (C++)
		- https://github.com/SaschaWillems/Vulkan
	- Cem Yuksel's Interactive Computer Graphics course
		- https://www.youtube.com/playlist?list=PLplnkTzzqsZS3R5DjmCQsqupu43oS9CFN
	- Johannes Unterguggenberger's Lecture Series
		- https://www.youtube.com/playlist?list=PLmIqTlJ6KsE1Jx5HV4sd2jOe3V1KMHHgn
	- The Cherno
		- https://www.youtube.com/@TheCherno




## Mouse Controls (starting in Chapter 15ish)
- Scroll to zoom
- Middle click to pan
- Middle + Right click to rotate (kinda like Catia)



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
<del>
- Sandbox03MeshShaders is using the Experimental build of Silk.NET 2.17 (as of 2023-03-30) 
	- get it here: https://dev.azure.com/UltzOS/Silk.NET/_artifacts/feed/Experimental
	- click "Connet to Feed", "Visual Studio", and copy the Source into a new nuget package source
</del>

- Silk.NET 2.17 is released, so make sure you're on that version at least.

- The projects require the [Vulkan SDK](https://www.lunarg.com/vulkan-sdk/) to build/run. The SDK provides the Vulkan validation layers as well as the command line tools to compile the shaders. 

- You will have to update your csproj files to point the folder where glslc.exe is found (this is the glsl compiler)

```xml
	<PropertyGroup>
		<VulkanBinPath>C:\VulkanSDK\1.3.239.0\Bin</VulkanBinPath>
	</PropertyGroup>
```
![screenshot of vulkan path](https://github.com/stymee/SilkVulkanTutorial/blob/master/Docs/screenshot3.png?raw=true)

** I couldn't get it to work with an environment variable, and I'm sure there's a better way to handle this globally


## Each chapter was re-coded in C# (.NET 7)
- Bonus projects:
	- Sandbox01Multisampling
	- Sandbox02ImGui
	- Sandbox03MeshShaders
	- Sandbox04ComputeShaders (coming soon)

![screenshot of solution](https://github.com/stymee/SilkVulkanTutorial/blob/master/Docs/screenshot2.png?raw=true)
