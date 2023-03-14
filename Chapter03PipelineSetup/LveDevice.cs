using Silk.NET;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Chapter03PipelineSetup;

public unsafe class LveDevice
{
    private readonly Vk vk = null!;
    private readonly LveWindow window;
    private Instance instance;

    private KhrSurface vkSurface;


    private ExtDebugUtils debugUtils = null!;
    private DebugUtilsMessengerEXT debugMessenger;

    private bool enableValidationLayers = true;
    private string[] validationLayers = { "VK_LAYER_KHRONOS_validation" };
    private List<string> instanceExtensions = new() { ExtDebugUtils.ExtensionName };
    private List<string> deviceExtensions = new() { KhrSwapchain.ExtensionName };

    public LveDevice(LveWindow window, Vk vk)
    {
        this.vk = vk;
        this.window = window;
        createInstance();
        setupDebugMessenger();
        createSurface();
        pickPhysicalDevice();
        createLogicalDevice();
        createCommandPool();
    }

    private void createInstance()
    {
        if (enableValidationLayers && !checkValidationLayerSupport())
        {
            throw new ApplicationException("Validation layers requested, but are not available!");
        }

        var appInfo = new ApplicationInfo()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("LittleVulkanEngine App"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("No Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version13
        };

        var createInfo = new InstanceCreateInfo()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        var extensions = window.VkSurface.GetRequiredExtensions(out var extCount);
        // TODO Review that this count doesn't realistically exceed 1k (recommended max for stack
        //
        //
        // )
        // Should probably be allocated on heap anyway as this isn't super performance critical.
        var newExtensions = stackalloc byte*[(int)(extCount + instanceExtensions.Count)];
        for (var i = 0; i < extCount; i++)
        {
            newExtensions[i] = extensions[i];
        }

        for (var i = 0; i < instanceExtensions.Count; i++)
        {
            newExtensions[extCount + i] = (byte*)SilkMarshal.StringToPtr(instanceExtensions[i]);
        }

        extCount += (uint)instanceExtensions.Count;
        createInfo.EnabledExtensionCount = extCount;
        createInfo.PpEnabledExtensionNames = newExtensions;

        if (enableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
            createInfo.PNext = null;
        }

        fixed (Instance* pInstance = &instance)
        {
            if (vk.CreateInstance(&createInfo, null, pInstance) != Result.Success)
            {
                throw new Exception("Failed to create instance!");
            }
        }

        vk.CurrentInstance = instance;

        if (!vk.TryGetInstanceExtension(instance, out vkSurface))
        {
            throw new NotSupportedException("KHR_surface extension not found.");
        }

        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);

        if (enableValidationLayers)
        {
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }

    }





    private bool checkValidationLayerSupport()
    {
        uint propCount = 0;
        var result = vk.EnumerateInstanceLayerProperties(ref propCount, null);
        if (propCount == 0)
        {
            return false;
        }

        var ret = false;
        using var mem = GlobalMemory.Allocate((int)propCount * sizeof(LayerProperties));
        var props = (LayerProperties*)Unsafe.AsPointer(ref mem.GetPinnableReference());
        vk.EnumerateInstanceLayerProperties(ref propCount, props);

        for (int i = 0; i < propCount; i++)
        {
            var layerName = GetString(props[i].LayerName);
            if (layerName == validationLayers[0]) ret = true;
            //Console.WriteLine($"{i} {layerName}");
        }
        return ret;
    }

    internal static unsafe string GetString(byte* stringStart)
    {
        int characters = 0;
        while (stringStart[characters] != 0)
        {
            characters++;
        }

        return Encoding.UTF8.GetString(stringStart, characters);
    }


    private void createSurface()
    {
        
    }


    private void pickPhysicalDevice()
    {
        
    }


    private void createLogicalDevice()
    {
        
    }


    private void createCommandPool()
    {
        
    }


    // debug stuff
    private void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
    {
        createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
        createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                     DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                     DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
        createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                 DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                 DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
        createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;
    }



    private void setupDebugMessenger()
    {
        if (!enableValidationLayers) return;

        //TryGetInstanceExtension equivilant to method CreateDebugUtilsMessengerEXT from original tutorial.
        if (!vk.TryGetInstanceExtension(instance, out debugUtils)) return;

        DebugUtilsMessengerCreateInfoEXT createInfo = new();
        PopulateDebugMessengerCreateInfo(ref createInfo);

        if (debugUtils!.CreateDebugUtilsMessenger(instance, in createInfo, null, out debugMessenger) != Result.Success)
        {
            throw new Exception("failed to set up debug messenger!");
        }
    }

    private uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
    {
        if (messageSeverity == DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt) return Vk.False;

        var msg = Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage);

        System.Diagnostics.Debug.WriteLine($"{messageSeverity} | validation layer: {msg}");

        return Vk.False;
    }

}
