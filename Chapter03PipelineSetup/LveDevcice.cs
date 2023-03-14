using Silk.NET;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using System.Runtime.InteropServices;

namespace Chapter03PipelineSetup;

public unsafe class LveDevcice
{
    //private Vk vk = null!;
    private Instance instance;
    private readonly LveWindow window;

    private ExtDebugUtils debugUtils = null!;
    private DebugUtilsMessengerEXT debugMessenger;

    private bool enableValidationLayers = true;

    public LveDevcice(LveWindow window)
    {
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
        if ()
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
        //if (!vk.TryGetInstanceExtension(instance, out debugUtils)) return;

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
