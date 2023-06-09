﻿
namespace Chapter27AlphaBlending;

public class LveRenderer : IDisposable
{

    // Vk api
    private readonly Vk vk = null!;
    private readonly IView window = null!;
    private readonly LveDevice device = null!;

    private LveSwapChain swapChain = null!;
    private CommandBuffer[] commandBuffers = null!;

    private bool framebufferResized = false;
    private uint currentImageIndex = 0;
    private int currentFrameIndex = 0;
    private bool isFrameStarted = false;

    //private LveSwapChain? oldSwapChain = null;

    // public props and methods
    public RenderPass GetSwapChainRenderPass() => swapChain.GetRenderPass();
    public float GetAspectRatio() => swapChain.GetAspectRatio();

    public bool IsFrameStarted => isFrameStarted;
    public CommandBuffer GetCurrentCommandBuffer() => commandBuffers[currentFrameIndex];
    public int GetFrameIndex() => currentFrameIndex;

    private bool useFifo = false;
    public bool UseFifo
    {
        get => useFifo;
        set
        {
            useFifo = value;
            swapChain.UseFifo = value;
        }
    }


    // Constructor
    public LveRenderer(Vk vk, IView window, LveDevice device, bool useFifo)
    {
        this.vk = vk;
        this.window = window;
        this.device = device;
        this.useFifo = useFifo;
        recreateSwapChain();
        createCommandBuffers();
    }



    private void recreateSwapChain()
    {
        var frameBufferSize = window.FramebufferSize;
        while (frameBufferSize.X == 0 || frameBufferSize.Y == 0)
        {
            frameBufferSize = window.FramebufferSize;
            window.DoEvents();
        }
        vk.DeviceWaitIdle(device.VkDevice);


        if (swapChain is null)
        {
            swapChain = new LveSwapChain(vk, device, GetWindowExtents(), useFifo);
        }
        else
        {
            //oldSwapChain = swapChain;
            //swapChain = new LveSwapChain(vk, device, GetWindowExtents(), oldSwapChain);

            //if (!oldSwapChain.CompareSwapFormats(swapChain))
            //{
            //    throw new Exception("Swap chain image(or depth) format has changed!");
            //}
            //if (swapChain.GetFrameBufferCount() != commandBuffers.Length)
            //{
            //    freeCommandBuffers();
            //    createCommandBuffers();
            //}
            var oldImageFormat = swapChain.SwapChainImageFormat;
            var oldDepthFormat = swapChain.SwapChainDepthFormat;

            swapChain.Dispose();
            swapChain = new LveSwapChain(vk, device, GetWindowExtents(), useFifo);

            if (swapChain.SwapChainImageFormat != oldImageFormat || swapChain.SwapChainDepthFormat != oldDepthFormat)
            {
                throw new Exception("Swap chain image(or depth) format has changed!");
            }
        }

    }

    private unsafe void createCommandBuffers()
    {
        //Array.Resize(ref commandBuffers, swapChain.MaxFramesInFlight);
        commandBuffers = new CommandBuffer[swapChain.ImageCount()];

        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = device.GetCommandPool(),
            CommandBufferCount = (uint)commandBuffers.Length,
        };

        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            if (vk!.AllocateCommandBuffers(device.VkDevice, allocInfo, commandBuffersPtr) != Result.Success)
            {
                throw new Exception("failed to allocate command buffers!");
            }
        }
    }


    private unsafe void freeCommandBuffers()
    {
        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            vk.FreeCommandBuffers(device.VkDevice, device.GetCommandPool(), (uint)commandBuffers.Length, commandBuffersPtr);
        }
        Array.Clear(commandBuffers);
    }





    //public bool BeginFrame(out CommandBuffer? outBuffer)
    public CommandBuffer? BeginFrame()
    {
        Debug.Assert(!isFrameStarted, "Can't call beginFrame while already in progress!");

        var result = swapChain.AcquireNextImage(ref currentImageIndex);

        if (result == Result.ErrorOutOfDateKhr)
        {
            recreateSwapChain();
            return null;
            //return default;
        }
        else if (result != Result.Success && result != Result.SuboptimalKhr)
        {
            throw new Exception("failed to acquire next swapchain image");
        }

        isFrameStarted = true;

        var commandBuffer = GetCurrentCommandBuffer();

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
        };

        if (vk.BeginCommandBuffer(commandBuffer, beginInfo) != Result.Success)
        {
            throw new Exception("failed to begin recording command buffer!");
        }

        return commandBuffer;
    }

    public void EndFrame()
    {
        Debug.Assert(isFrameStarted, "Can't call endFrame while frame is not in progress");

        var commandBuffer = GetCurrentCommandBuffer();

        if (vk.EndCommandBuffer(commandBuffer) != Result.Success)
        {
            throw new Exception("failed to record command buffer!");
        }

        var result = swapChain.SubmitCommandBuffers(commandBuffer, currentImageIndex);
        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || framebufferResized)
        {
            framebufferResized = false;
            recreateSwapChain();
        }
        else if (result != Result.Success)
        {
            throw new Exception("failed to submit command buffers");
        }

        isFrameStarted = false;
        currentFrameIndex = (currentFrameIndex + 1) % swapChain.MaxFramesInFlight;
    }

    public unsafe void BeginSwapChainRenderPass(CommandBuffer commandBuffer)
    {
        Debug.Assert(isFrameStarted, "Can't call beginSwapChainRenderPass if frame is not in progress");
        Debug.Assert(commandBuffer.Handle == GetCurrentCommandBuffer().Handle, "Can't begin render pass on command buffer from a different frame");

        RenderPassBeginInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = swapChain.GetRenderPass(),
            Framebuffer = swapChain.GetFrameBufferAt(currentImageIndex),
            RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = swapChain.GetSwapChainExtent(),
                }
        };

        var clearValues = new ClearValue[]
        {
                new()
                {
                    Color = new (){ Float32_0 = 0.01f, Float32_1 = 0.01f, Float32_2 = 0.01f, Float32_3 = 1 },
                },
                new()
                {
                    DepthStencil = new () { Depth = 1, Stencil = 0 }
                }
        };


        fixed (ClearValue* clearValuesPtr = clearValues)
        {
            renderPassInfo.ClearValueCount = (uint)clearValues.Length;
            renderPassInfo.PClearValues = clearValuesPtr;

            vk.CmdBeginRenderPass(commandBuffer, &renderPassInfo, SubpassContents.Inline);
        }


        Viewport viewport = new()
        {
            X = 0.0f,
            Y = 0.0f,
            Width = swapChain.GetSwapChainExtent().Width,
            Height = swapChain.GetSwapChainExtent().Height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f,
        };
        Rect2D scissor = new(new Offset2D(), swapChain.GetSwapChainExtent());
        vk.CmdSetViewport(commandBuffer, 0, 1, &viewport);
        vk.CmdSetScissor(commandBuffer, 0, 1, &scissor);

    }

    public void EndSwapChainRenderPass(CommandBuffer commandBuffer)
    {
        Debug.Assert(isFrameStarted, "Can't call endSwapChainRenderPass if frame is not in progress");
        Debug.Assert(commandBuffer.Handle == GetCurrentCommandBuffer().Handle, "Can't end render pass on command buffer from a different frame");

        vk.CmdEndRenderPass(commandBuffer);

    }





    private Extent2D GetWindowExtents()
    {
        return new Extent2D((uint)window.FramebufferSize.X, (uint)window.FramebufferSize.Y);
    }

    public unsafe void Dispose()
    {
        freeCommandBuffers();
        GC.SuppressFinalize(this);
    }

}