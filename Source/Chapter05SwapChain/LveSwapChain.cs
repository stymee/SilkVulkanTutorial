﻿
namespace Chapter05SwapChain;

public class LveSwapChain : IDisposable
{
    private bool disposedValue;

    const int MAX_FRAMES_IN_FLIGHT = 2;

    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private readonly Device vkDevice;

    private KhrSwapchain khrSwapChain = null!;
    private SwapchainKHR swapChain;
    private Image[] swapChainImages = null!;
    private Format swapChainImageFormat;
    private Extent2D swapChainExtent;
    private ImageView[] swapChainImageViews = null!;
    private Framebuffer[] swapChainFramebuffers = null!;
    public Framebuffer GetFrameBufferAt(int i) => swapChainFramebuffers[i];


    //private Queue graphicsQueue;
    //private Queue presentQueue;

    private bool framebufferResized = false;

    // save this for later
    //private SampleCountFlags msaaSamples = SampleCountFlags.Count1Bit;

    private RenderPass renderPass;
    //private DescriptorSetLayout descriptorSetLayout;
    //private PipelineLayout pipelineLayout;
    //private Pipeline graphicsPipeline;

    //private CommandPool commandPool;

    //private Image colorImage;
    //private DeviceMemory colorImageMemory;
    //private ImageView colorImageView;

    private Image[] depthImages = null!;
    private DeviceMemory[] depthImageMemorys = null!;
    private ImageView[] depthImageViews = null!;

    private Extent2D windowExtent;

    private Semaphore[] imageAvailableSemaphores = null!;
    private Semaphore[] renderFinishedSemaphores = null!;
    private Fence[] inFlightFences = null!;
    private Fence[] imagesInFlight = null!;
    private int currentFrame = 0;

    public uint Width => swapChainExtent.Width;
    public uint Height => swapChainExtent.Height;

    public Extent2D GetSwapChainExtent() => swapChainExtent;
    public RenderPass GetRenderPass() => renderPass;

    public uint ImageCount() => (uint)swapChainImageViews.Length;

    public LveSwapChain(Vk vk, LveDevice device, Extent2D extent)
    {
        this.vk = vk;
        this.device = device;
        vkDevice = device.VkDevice;
        windowExtent = extent;
        createSwapChain();
        createImageViews();
        createRenderPass();
        createDepthResources();
        createFrameBuffers();
        createSyncObjects();
    }


    public unsafe Result AcquireNextImage(uint imageIndex)
    {
        var fence = inFlightFences[currentFrame];
        vk.WaitForFences(device.VkDevice, 1, in fence, Vk.True, ulong.MaxValue);

        Result result = khrSwapChain.AcquireNextImage
            (device.VkDevice, swapChain, ulong.MaxValue, imageAvailableSemaphores[currentFrame], default, &imageIndex);

        return result;
    }


    public unsafe Result SubmitCommandBuffers(CommandBuffer commandBuffer, uint imageIndex)
    {
        if (imagesInFlight[imageIndex].Handle != 0)
        {
            vk.WaitForFences(device.VkDevice, 1, in imagesInFlight[imageIndex], Vk.True, ulong.MaxValue);
        }

        imagesInFlight[imageIndex] = inFlightFences[currentFrame];

        Result result = Result.NotReady;

        var submitInfo = new SubmitInfo { SType = StructureType.SubmitInfo };
        Semaphore[] waitSemaphores = { imageAvailableSemaphores[currentFrame] };
        PipelineStageFlags[] waitStages = { PipelineStageFlags.ColorAttachmentOutputBit };
        submitInfo.WaitSemaphoreCount = 1;
        var signalSemaphore = renderFinishedSemaphores[currentFrame];
        fixed (Semaphore* waitSemaphoresPtr = waitSemaphores)
        {
            fixed (PipelineStageFlags* waitStagesPtr = waitStages)
            {
                submitInfo.PWaitSemaphores = waitSemaphoresPtr;
                submitInfo.PWaitDstStageMask = waitStagesPtr;

                submitInfo.CommandBufferCount = 1;
                var buffer = commandBuffer;
                submitInfo.PCommandBuffers = &buffer;

                submitInfo.SignalSemaphoreCount = 1;
                submitInfo.PSignalSemaphores = &signalSemaphore;

                vk.ResetFences(device.VkDevice, 1, inFlightFences[currentFrame]);

                if (vk.QueueSubmit
                        (device.GraphicsQueue, 1, &submitInfo, inFlightFences[currentFrame]) != Result.Success)
                {
                    throw new Exception("failed to submit draw command buffer!");
                }
            }
        }

        fixed (SwapchainKHR* swapChainPtr = &swapChain)
        {
            PresentInfoKHR presentInfo = new PresentInfoKHR
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = &signalSemaphore,
                SwapchainCount = 1,
                PSwapchains = swapChainPtr,
                PImageIndices = &imageIndex
            };

            result = khrSwapChain.QueuePresent(device.PresentQueue, &presentInfo);
        }

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || framebufferResized)
        {
            framebufferResized = false;
            //RecreateSwapChain();
        }
        else if (result != Result.Success)
        {
            throw new Exception("failed to present swap chain image!");
        }

        currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
        return Result.Success;
    }

    private unsafe void createSwapChain()
    {
        var swapChainSupport = device.QuerySwapChainSupport();

        var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
        var extent = ChooseSwapExtent(swapChainSupport.Capabilities);

        var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
        if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
        {
            imageCount = swapChainSupport.Capabilities.MaxImageCount;
        }

        SwapchainCreateInfoKHR creatInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = device.Surface,

            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
        };

        var indices = device.FindQueueFamilies();
        var queueFamilyIndices = stackalloc[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };

        if (indices.GraphicsFamily != indices.PresentFamily)
        {
            creatInfo = creatInfo with
            {
                ImageSharingMode = SharingMode.Concurrent,
                QueueFamilyIndexCount = 2,
                PQueueFamilyIndices = queueFamilyIndices,
            };
        }
        else
        {
            creatInfo.ImageSharingMode = SharingMode.Exclusive;
        }

        creatInfo = creatInfo with
        {
            PreTransform = swapChainSupport.Capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true,
        };

        if (khrSwapChain is null)
        {
            if (!vk.TryGetDeviceExtension(device.Instance, vkDevice, out khrSwapChain))
            {
                throw new NotSupportedException("VK_KHR_swapchain extension not found.");
            }
        }

        if (khrSwapChain.CreateSwapchain(vkDevice, creatInfo, null, out swapChain) != Result.Success)
        {
            throw new Exception("failed to create swap chain!");
        }

        khrSwapChain.GetSwapchainImages(vkDevice, swapChain, ref imageCount, null);
        swapChainImages = new Image[imageCount];
        fixed (Image* swapChainImagesPtr = swapChainImages)
        {
            khrSwapChain.GetSwapchainImages(vkDevice, swapChain, ref imageCount, swapChainImagesPtr);
        }

        swapChainImageFormat = surfaceFormat.Format;
        swapChainExtent = extent;
    }


    private void createImageViews()
    {
        swapChainImageViews = new ImageView[swapChainImages!.Length];

        for (int i = 0; i < swapChainImages.Length; i++)
        {

            swapChainImageViews[i] = CreateImageView(swapChainImages[i], swapChainImageFormat, ImageAspectFlags.ColorBit, 1);
        }

    }


    private unsafe void createRenderPass()
    {
        AttachmentDescription depthAttachment = new()
        {
            Format = device.FindDepthFormat(),
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
        };

        AttachmentReference depthAttachmentRef = new()
        {
            Attachment = 1,
            Layout = ImageLayout.DepthStencilAttachmentOptimal,
        };

        AttachmentDescription colorAttachment = new()
        {
            Format = swapChainImageFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr,
        };

        AttachmentReference colorAttachmentRef = new()
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal,
        };


        SubpassDescription subpass = new()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
            PDepthStencilAttachment = &depthAttachmentRef,
        };

        SubpassDependency dependency = new()
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            SrcAccessMask = 0,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
        };

        var attachments = new[] { colorAttachment, depthAttachment };

        fixed (AttachmentDescription* attachmentsPtr = attachments)
        {
            RenderPassCreateInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = (uint)attachments.Length,
                PAttachments = attachmentsPtr,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency,
            };

            if (vk.CreateRenderPass(vkDevice, renderPassInfo, null, out renderPass) != Result.Success)
            {
                throw new Exception("failed to create render pass!");
            }
        }
    }


    private unsafe void createFrameBuffers()
    {
        swapChainFramebuffers = new Framebuffer[swapChainImageViews.Length];

        for (int i = 0; i < swapChainImageViews.Length; i++)
        {
            var attachments = new[] { swapChainImageViews[i], depthImageViews[i] };

            fixed (ImageView* attachmentsPtr = attachments)
            {
                FramebufferCreateInfo framebufferInfo = new()
                {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = renderPass,
                    AttachmentCount = (uint)attachments.Length,
                    PAttachments = attachmentsPtr,
                    Width = swapChainExtent.Width,
                    Height = swapChainExtent.Height,
                    Layers = 1,
                };

                if (vk!.CreateFramebuffer(device.VkDevice, framebufferInfo, null, out swapChainFramebuffers[i]) != Result.Success)
                {
                    throw new Exception("failed to create framebuffer!");
                }
            }
        }
    }


    private unsafe void createDepthResources()
    {
        Format depthFormat = device.FindDepthFormat();

        var imageCount = ImageCount();
        depthImages = new Image[imageCount];
        depthImageMemorys = new DeviceMemory[imageCount];
        depthImageViews = new ImageView[imageCount];

        for (int i = 0; i < imageCount; i++)
        {
            ImageCreateInfo imageInfo = new()
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent =
            {
                Width = swapChainExtent.Width,
                Height = swapChainExtent.Height,
                Depth = 1,
            },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = depthFormat,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.DepthStencilAttachmentBit,
                Samples = SampleCountFlags.Count1Bit,
                SharingMode = SharingMode.Exclusive,
                Flags = 0
            };

            fixed (Image* imagePtr = &depthImages[i])
            {
                if (vk.CreateImage(vkDevice, imageInfo, null, imagePtr) != Result.Success)
                {
                    throw new Exception("failed to create depth image!");
                }
            }

            MemoryRequirements memRequirements;
            vk.GetImageMemoryRequirements(vkDevice, depthImages[i], out memRequirements);

            MemoryAllocateInfo allocInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = device.FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };

            fixed (DeviceMemory* imageMemoryPtr = &depthImageMemorys[i])
            {
                if (vk.AllocateMemory(vkDevice, allocInfo, null, imageMemoryPtr) != Result.Success)
                {
                    throw new Exception("failed to allocate depth image memory!");
                }
            }

            vk.BindImageMemory(vkDevice, depthImages[i], depthImageMemorys[i], 0);


            // depth image view
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = depthImages[i],
                ViewType = ImageViewType.Type2D,
                Format = depthFormat,
                //Components =
                //    {
                //        R = ComponentSwizzle.Identity,
                //        G = ComponentSwizzle.Identity,
                //        B = ComponentSwizzle.Identity,
                //        A = ComponentSwizzle.Identity,
                //    },
                SubresourceRange =
                {
                    AspectMask = ImageAspectFlags.DepthBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

            };

            if (vk.CreateImageView(vkDevice, createInfo, null, out depthImageViews[i]) != Result.Success)
            {
                throw new Exception("failed to create depth image views!");
            }



        }

    }


    private unsafe void CreateImage(uint width, uint height, uint mipLevels, SampleCountFlags numSamples, Format format, ImageTiling tiling, ImageUsageFlags usage, MemoryPropertyFlags properties, ref Image image, ref DeviceMemory imageMemory)
    {
        ImageCreateInfo imageInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent =
            {
                Width = width,
                Height = height,
                Depth = 1,
            },
            MipLevels = mipLevels,
            ArrayLayers = 1,
            Format = format,
            Tiling = tiling,
            InitialLayout = ImageLayout.Undefined,
            Usage = usage,
            Samples = numSamples,
            SharingMode = SharingMode.Exclusive,
        };

        fixed (Image* imagePtr = &image)
        {
            if (vk.CreateImage(vkDevice, imageInfo, null, imagePtr) != Result.Success)
            {
                throw new Exception("failed to create image!");
            }
        }

        MemoryRequirements memRequirements;
        vk.GetImageMemoryRequirements(vkDevice, image, out memRequirements);

        MemoryAllocateInfo allocInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = device.FindMemoryType(memRequirements.MemoryTypeBits, properties),
        };

        fixed (DeviceMemory* imageMemoryPtr = &imageMemory)
        {
            if (vk.AllocateMemory(vkDevice, allocInfo, null, imageMemoryPtr) != Result.Success)
            {
                throw new Exception("failed to allocate image memory!");
            }
        }

        vk.BindImageMemory(vkDevice, image, imageMemory, 0);
    }

    private unsafe ImageView CreateImageView(Image image, Format format, ImageAspectFlags aspectFlags, uint mipLevels)
    {
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            ViewType = ImageViewType.Type2D,
            Format = format,
            //Components =
            //    {
            //        R = ComponentSwizzle.Identity,
            //        G = ComponentSwizzle.Identity,
            //        B = ComponentSwizzle.Identity,
            //        A = ComponentSwizzle.Identity,
            //    },
            SubresourceRange =
                {
                    AspectMask = aspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = mipLevels,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

        };

        ImageView imageView = default;

        if (vk.CreateImageView(vkDevice, createInfo, null, out imageView) != Result.Success)
        {
            throw new Exception("failed to create image views!");
        }

        return imageView;
    }

    private unsafe void createSyncObjects()
    {
        imageAvailableSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
        renderFinishedSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
        inFlightFences = new Fence[MAX_FRAMES_IN_FLIGHT];
        imagesInFlight = new Fence[swapChainImages!.Length];

        SemaphoreCreateInfo semaphoreInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo,
        };

        FenceCreateInfo fenceInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit,
        };

        for (var i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
        {
            if (vk.CreateSemaphore(vkDevice, semaphoreInfo, null, out imageAvailableSemaphores[i]) != Result.Success ||
                vk.CreateSemaphore(vkDevice, semaphoreInfo, null, out renderFinishedSemaphores[i]) != Result.Success ||
                vk.CreateFence(vkDevice, fenceInfo, null, out inFlightFences[i]) != Result.Success)
            {
                throw new Exception("failed to create synchronization objects for a frame!");
            }
        }
    }


    private SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
    {
        foreach (var availableFormat in availableFormats)
        {
            if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                return availableFormat;
            }
        }

        return availableFormats[0];
    }

    private PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes)
    {
        foreach (var availablePresentMode in availablePresentModes)
        {
            if (availablePresentMode == PresentModeKHR.MailboxKhr)
            {
                log.d("swapchain", $"got present mode = Mailbox");
                return availablePresentMode;
            }
        }

        log.d("swapchain", $"fallback present mode = FifoKhr");
        return PresentModeKHR.FifoKhr;
    }

    private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }
        else
        {
            var framebufferSize = windowExtent;

            Extent2D actualExtent = new()
            {
                Width = framebufferSize.Width,
                Height = framebufferSize.Height
            };

            actualExtent.Width = Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
            actualExtent.Height = Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

            return actualExtent;
        }
    }


    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~LveSwapChain()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
