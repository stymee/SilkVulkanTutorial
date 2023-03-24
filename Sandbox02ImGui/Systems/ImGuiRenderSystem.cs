
using Silk.NET.Vulkan;

namespace Sandbox02ImGui;

public class ImGuiRenderSystem : IDisposable
{
    private readonly Vk vk = null!;
    private readonly LveDevice device = null!;
    private bool disposedValue;

    private LvePipeline pipeline = null!;
    private PipelineLayout pipelineLayout;

    // ImGui init stuff
    private IView view = null!;
    private IInputContext input = null!;
    private PhysicalDevice physicalDevice;
    private bool _frameBegun;
    private uint graphicsFamilyIndex;
    private int windowWidth;
    private int windowHeight;
    private int swapChainImageCt;
    private RenderPass _renderPass;

    // other ImGui stuff
    private DescriptorPool _descriptorPool;
    private Sampler _fontSampler;
    private DeviceMemory _fontMemory;
    private Image _fontImage;
    private ImageView _fontView;
    private ulong _bufferMemoryAlignment = 256;
    private readonly List<char> _pressedChars = new List<char>();
    private IKeyboard _keyboard = null!;

    private WindowRenderBuffers _mainWindowRenderBuffers;
    private GlobalMemory _frameRenderBuffers;

    private DescriptorSetLayout _descriptorSetLayout;
    private DescriptorSet _descriptorSet;

    //private Vector2D<int> windowSize;
    private Vector2D<int> framebufferSize;

    public ImGuiRenderSystem(Vk vk, LveDevice device, RenderPass renderPass, DescriptorSetLayout globalSetLayout, IWindow window)
    {
        this.vk = vk;
        this.device = device;
        view = window;
        input = window.CreateInput();
        physicalDevice = device.VkPhysicalDevice;
        graphicsFamilyIndex = device.GraphicsFamilyIndex;
        swapChainImageCt = LveSwapChain.MAX_FRAMES_IN_FLIGHT;
        _renderPass = renderPass;

        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        var io = ImGui.GetIO();
        io.Fonts.AddFontDefault();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        windowWidth = window.Size.X;
        windowHeight = window.Size.Y;
        framebufferSize = window.FramebufferSize;

        // Set default style
        ImGui.StyleColorsDark();

        createPipelineLayout(globalSetLayout);
        createPipeline(renderPass);

        InitAdapter();

        SetKeyMappings();

        SetPerFrameImGuiData(1f / 60f);
    }
    private void BeginFrame()
    {
        ImGui.NewFrame();
        _frameBegun = true;
        _keyboard = input.Keyboards[0];
        view.Resize += WindowResized;
        _keyboard.KeyChar += OnKeyChar;
    }
    private void OnKeyChar(IKeyboard arg1, char arg2)
    {
        _pressedChars.Add(arg2);
    }

    private void WindowResized(Vector2D<int> size)
    {
        windowWidth = size.X;
        windowHeight = size.Y;
    }


    public unsafe void Render(FrameInfo frameInfo, Framebuffer framebuffer, Extent2D swapChainExtent)
    {
        pipeline.Bind(frameInfo.CommandBuffer);

        vk.CmdBindDescriptorSets(
            frameInfo.CommandBuffer,
            PipelineBindPoint.Graphics,
            pipelineLayout,
            0,
            1,
            frameInfo.GlobalDescriptorSet,
            0,
            null
        );

        if (_frameBegun)
        {
            _frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData(), frameInfo.CommandBuffer, framebuffer, swapChainExtent);
        }

    }


    public void Update(float deltaSeconds)
    {
        if (_frameBegun)
        {
            ImGuiNET.ImGui.Render();
        }

        SetPerFrameImGuiData(deltaSeconds);
        UpdateImGuiInput();

        _frameBegun = true;
        ImGuiNET.ImGui.NewFrame();
    }


    private unsafe void createPipelineLayout(DescriptorSetLayout globalSetLayout)
    {
        Span<DescriptorPoolSize> poolSizes = stackalloc DescriptorPoolSize[] { new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 1) };
        var descriptorPool = new DescriptorPoolCreateInfo();
        descriptorPool.SType = StructureType.DescriptorPoolCreateInfo;
        descriptorPool.PoolSizeCount = (uint)poolSizes.Length;
        descriptorPool.PPoolSizes = (DescriptorPoolSize*)Unsafe.AsPointer(ref poolSizes.GetPinnableReference());
        descriptorPool.MaxSets = 1;
        if (vk.CreateDescriptorPool(device.VkDevice, descriptorPool, default, out _descriptorPool) != Result.Success)
        {
            throw new Exception($"Unable to create descriptor pool");
        }

        var info = new SamplerCreateInfo();
        info.SType = StructureType.SamplerCreateInfo;
        info.MagFilter = Filter.Linear;
        info.MinFilter = Filter.Linear;
        info.MipmapMode = SamplerMipmapMode.Linear;
        info.AddressModeU = SamplerAddressMode.Repeat;
        info.AddressModeV = SamplerAddressMode.Repeat;
        info.AddressModeW = SamplerAddressMode.Repeat;
        info.MinLod = -1000;
        info.MaxLod = 1000;
        info.MaxAnisotropy = 1.0f;
        if (vk.CreateSampler(device.VkDevice, info, default, out _fontSampler) != Result.Success)
        {
            throw new Exception($"Unable to create sampler");
        }

        var sampler = _fontSampler;

        var binding = new DescriptorSetLayoutBinding();
        binding.DescriptorType = DescriptorType.CombinedImageSampler;
        binding.DescriptorCount = 1;
        binding.StageFlags = ShaderStageFlags.FragmentBit;
        binding.PImmutableSamplers = (Sampler*)Unsafe.AsPointer(ref sampler);

        var descriptorInfo = new DescriptorSetLayoutCreateInfo();
        descriptorInfo.SType = StructureType.DescriptorSetLayoutCreateInfo;
        descriptorInfo.BindingCount = 1;
        descriptorInfo.PBindings = (DescriptorSetLayoutBinding*)Unsafe.AsPointer(ref binding);
        if (vk.CreateDescriptorSetLayout(device.VkDevice, descriptorInfo, default, out _descriptorSetLayout) != Result.Success)
        {
            throw new Exception($"Unable to create descriptor set layout");
        }

        fixed (DescriptorSetLayout* pg_DescriptorSetLayout = &_descriptorSetLayout)
        {
            var alloc_info = new DescriptorSetAllocateInfo();
            alloc_info.SType = StructureType.DescriptorSetAllocateInfo;
            alloc_info.DescriptorPool = _descriptorPool;
            alloc_info.DescriptorSetCount = 1;
            alloc_info.PSetLayouts = pg_DescriptorSetLayout;
            if (vk.AllocateDescriptorSets(device.VkDevice, alloc_info, out _descriptorSet) != Result.Success)
            {
                throw new Exception($"Unable to create descriptor sets");
            }
        }


        var descriptorSetLayouts = new DescriptorSetLayout[] { _descriptorSetLayout };
        PushConstantRange pushConstantRange = new()
        {
            StageFlags = ShaderStageFlags.VertexBit,
            Offset = sizeof(float) * 0,
            Size = sizeof(float) * 4,
        };


        fixed (DescriptorSetLayout* descriptorSetLayoutPtr = descriptorSetLayouts)
        {
            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)descriptorSetLayouts.Length,
                PSetLayouts = descriptorSetLayoutPtr,
                PushConstantRangeCount = 1,
                PPushConstantRanges = &pushConstantRange,
            };

            if (vk.CreatePipelineLayout(device.VkDevice, pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            {
                throw new Exception("failed to create pipeline layout!");
            }
        }
    }


    private unsafe void createPipeline(RenderPass renderPass)
    {
        Debug.Assert(pipelineLayout.Handle != 0, "Cannot create pipeline before pipeline layout");

        var pipelineConfig = LvePipeline.GetDefaultPipelineConfigInfo();

        LvePipeline.EnableMultiSampling(ref pipelineConfig, device.GetMsaaSamples());

        var bindingInfo = new VertexInputBindingDescription[]
        {
            new()
            {
                Stride = (uint)Unsafe.SizeOf<ImDrawVert>(),
                InputRate = VertexInputRate.Vertex
            }
        };

        var attributeInfo = new VertexInputAttributeDescription[]
        {
            new()
            {
                Binding = bindingInfo[0].Binding,
                Location = 0,
                Format = Format.R32G32Sfloat,
                Offset = (uint)Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.pos))
            },
            new()
            {
                Binding = bindingInfo[0].Binding,
                Location = 1,
                Format = Format.R32G32Sfloat,
                Offset = (uint)Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.uv))
            },
            new()
            {
                Binding = bindingInfo[0].Binding,
                Location = 2,
                Format = Format.R8G8B8A8Unorm,
                Offset = (uint) Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.col))
            }
        };

        pipelineConfig.BindingDescriptions = Array.Empty<VertexInputBindingDescription>();
        pipelineConfig.AttributeDescriptions = Array.Empty<VertexInputAttributeDescription>();
        pipelineConfig.BindingDescriptions = bindingInfo;
        pipelineConfig.AttributeDescriptions = attributeInfo;

        pipelineConfig.RenderPass = renderPass;
        pipelineConfig.PipelineLayout = pipelineLayout;

        pipeline = new LvePipeline(
            vk, device,
            "imGui.vert.spv",
            "imGui.frag.spv",
            pipelineConfig
            );




    }


    public unsafe void InitAdapter()
    {
        CommandBuffer commandBuffer = device.BeginSingleTimeCommands();

        // Initialise ImGui Vulkan adapter
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height);
        ulong upload_size = (ulong)(width * height * 4 * sizeof(byte));
        var imageInfo = new ImageCreateInfo();

        imageInfo.SType = StructureType.ImageCreateInfo;
        imageInfo.ImageType = ImageType.Type2D;
        imageInfo.Format = Format.R8G8B8A8Unorm;
        imageInfo.Extent.Width = (uint)width;
        imageInfo.Extent.Height = (uint)height;
        imageInfo.Extent.Depth = 1;
        imageInfo.MipLevels = 1;
        imageInfo.ArrayLayers = 1;
        imageInfo.Samples = SampleCountFlags.Count1Bit;
        imageInfo.Tiling = ImageTiling.Optimal;
        imageInfo.Usage = ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit;
        imageInfo.SharingMode = SharingMode.Exclusive;
        imageInfo.InitialLayout = ImageLayout.Undefined;
        if (vk.CreateImage(device.VkDevice, imageInfo, default, out _fontImage) != Result.Success)
        {
            throw new Exception($"Failed to create font image");
        }
        vk.GetImageMemoryRequirements(device.VkDevice, _fontImage, out var fontReq);
        var fontAllocInfo = new MemoryAllocateInfo();
        fontAllocInfo.SType = StructureType.MemoryAllocateInfo;
        fontAllocInfo.AllocationSize = fontReq.Size;
        fontAllocInfo.MemoryTypeIndex = GetMemoryTypeIndex(vk, MemoryPropertyFlags.DeviceLocalBit, fontReq.MemoryTypeBits);
        if (vk.AllocateMemory(device.VkDevice, &fontAllocInfo, default, out _fontMemory) != Result.Success)
        {
            throw new Exception($"Failed to allocate device memory");
        }
        if (vk.BindImageMemory(device.VkDevice, _fontImage, _fontMemory, 0) != Result.Success)
        {
            throw new Exception($"Failed to bind device memory");
        }

        var imageViewInfo = new ImageViewCreateInfo();
        imageViewInfo.SType = StructureType.ImageViewCreateInfo;
        imageViewInfo.Image = _fontImage;
        imageViewInfo.ViewType = ImageViewType.Type2D;
        imageViewInfo.Format = Format.R8G8B8A8Unorm;
        imageViewInfo.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
        imageViewInfo.SubresourceRange.LevelCount = 1;
        imageViewInfo.SubresourceRange.LayerCount = 1;
        if (vk.CreateImageView(device.VkDevice, &imageViewInfo, default, out _fontView) != Result.Success)
        {
            throw new Exception($"Failed to create an image view");
        }

        var descImageInfo = new DescriptorImageInfo();
        descImageInfo.Sampler = _fontSampler;
        descImageInfo.ImageView = _fontView;
        descImageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;
        var writeDescriptors = new WriteDescriptorSet();
        writeDescriptors.SType = StructureType.WriteDescriptorSet;
        writeDescriptors.DstSet = _descriptorSet;
        writeDescriptors.DescriptorCount = 1;
        writeDescriptors.DescriptorType = DescriptorType.CombinedImageSampler;
        writeDescriptors.PImageInfo = (DescriptorImageInfo*)Unsafe.AsPointer(ref descImageInfo);
        vk.UpdateDescriptorSets(device.VkDevice, 1, writeDescriptors, 0, default);

        // Create the Upload Buffer:
        var bufferInfo = new BufferCreateInfo();
        bufferInfo.SType = StructureType.BufferCreateInfo;
        bufferInfo.Size = upload_size;
        bufferInfo.Usage = BufferUsageFlags.TransferSrcBit;
        bufferInfo.SharingMode = SharingMode.Exclusive;
        if (vk.CreateBuffer(device.VkDevice, bufferInfo, default, out var uploadBuffer) != Result.Success)
        {
            throw new Exception($"Failed to create a device buffer");
        }

        vk.GetBufferMemoryRequirements(device.VkDevice, uploadBuffer, out var uploadReq);
        _bufferMemoryAlignment = (_bufferMemoryAlignment > uploadReq.Alignment) ? _bufferMemoryAlignment : uploadReq.Alignment;

        var uploadAllocInfo = new MemoryAllocateInfo();
        uploadAllocInfo.SType = StructureType.MemoryAllocateInfo;
        uploadAllocInfo.AllocationSize = uploadReq.Size;
        uploadAllocInfo.MemoryTypeIndex = GetMemoryTypeIndex(vk, MemoryPropertyFlags.HostVisibleBit, uploadReq.MemoryTypeBits);
        if (vk.AllocateMemory(device.VkDevice, uploadAllocInfo, default, out var uploadBufferMemory) != Result.Success)
        {
            throw new Exception($"Failed to allocate device memory");
        }
        if (vk.BindBufferMemory(device.VkDevice, uploadBuffer, uploadBufferMemory, 0) != Result.Success)
        {
            throw new Exception($"Failed to bind device memory");
        }

        void* map = null;
        if (vk.MapMemory(device.VkDevice, uploadBufferMemory, 0, upload_size, 0, (void**)(&map)) != Result.Success)
        {
            throw new Exception($"Failed to map device memory");
        }
        Unsafe.CopyBlock(map, pixels.ToPointer(), (uint)upload_size);

        var range = new MappedMemoryRange();
        range.SType = StructureType.MappedMemoryRange;
        range.Memory = uploadBufferMemory;
        range.Size = upload_size;
        if (vk.FlushMappedMemoryRanges(device.VkDevice, 1, range) != Result.Success)
        {
            throw new Exception($"Failed to flush memory to device");
        }
        vk.UnmapMemory(device.VkDevice, uploadBufferMemory);

        const uint VK_QUEUE_FAMILY_IGNORED = ~0U;

        var copyBarrier = new ImageMemoryBarrier();
        copyBarrier.SType = StructureType.ImageMemoryBarrier;
        copyBarrier.DstAccessMask = AccessFlags.TransferWriteBit;
        copyBarrier.OldLayout = ImageLayout.Undefined;
        copyBarrier.NewLayout = ImageLayout.TransferDstOptimal;
        copyBarrier.SrcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
        copyBarrier.DstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
        copyBarrier.Image = _fontImage;
        copyBarrier.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
        copyBarrier.SubresourceRange.LevelCount = 1;
        copyBarrier.SubresourceRange.LayerCount = 1;
        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.HostBit, PipelineStageFlags.TransferBit, 0, 0, default, 0, default, 1, copyBarrier);

        var region = new BufferImageCopy();
        region.ImageSubresource.AspectMask = ImageAspectFlags.ColorBit;
        region.ImageSubresource.LayerCount = 1;
        region.ImageExtent.Width = (uint)width;
        region.ImageExtent.Height = (uint)height;
        region.ImageExtent.Depth = 1;
        vk.CmdCopyBufferToImage(commandBuffer, uploadBuffer, _fontImage, ImageLayout.TransferDstOptimal, 1, &region);

        var use_barrier = new ImageMemoryBarrier();
        use_barrier.SType = StructureType.ImageMemoryBarrier;
        use_barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
        use_barrier.DstAccessMask = AccessFlags.ShaderReadBit;
        use_barrier.OldLayout = ImageLayout.TransferDstOptimal;
        use_barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
        use_barrier.SrcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
        use_barrier.DstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
        use_barrier.Image = _fontImage;
        use_barrier.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
        use_barrier.SubresourceRange.LevelCount = 1;
        use_barrier.SubresourceRange.LayerCount = 1;
        vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit, 0, 0, default, 0, default, 1, use_barrier);

        // Store our identifier
        io.Fonts.SetTexID((IntPtr)_fontImage.Handle);

        device.EndSingleTimeCommands(commandBuffer);
    }

    private uint GetMemoryTypeIndex(Vk vk, MemoryPropertyFlags properties, uint type_bits)
    {
        vk.GetPhysicalDeviceMemoryProperties(device.VkPhysicalDevice, out var prop);
        for (int i = 0; i < prop.MemoryTypeCount; i++)
        {
            if ((prop.MemoryTypes[i].PropertyFlags & properties) == properties && (type_bits & (1u << i)) != 0)
            {
                return (uint)i;
            }
        }
        return 0xFFFFFFFF; // Unable to find memoryType
    }




    private void SetPerFrameImGuiData(float deltaSeconds)
    {
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(windowWidth, windowHeight);

        if (windowWidth > 0 && windowHeight > 0)
        {
            io.DisplayFramebufferScale = new Vector2(framebufferSize.X / windowWidth, framebufferSize.Y / windowHeight);
        }
        io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
    }

    private void UpdateImGuiInput()
    {
        var io = ImGui.GetIO();

        var mouseState = input.Mice[0].CaptureState();
        var keyboardState = input.Keyboards[0];

        io.MouseDown[0] = mouseState.IsButtonPressed(MouseButton.Left);
        io.MouseDown[1] = mouseState.IsButtonPressed(MouseButton.Right);
        io.MouseDown[2] = mouseState.IsButtonPressed(MouseButton.Middle);

        var point = new Point((int)mouseState.Position.X, (int)mouseState.Position.Y);
        io.MousePos = new Vector2(point.X, point.Y);

        var wheel = mouseState.GetScrollWheels()[0];
        io.MouseWheel = wheel.Y;
        io.MouseWheelH = wheel.X;

        foreach (Key key in Enum.GetValues(typeof(Key)))
        {
            if (key == Key.Unknown)
            {
                continue;
            }
            io.KeysDown[(int)key] = keyboardState.IsKeyPressed(key);
        }

        foreach (var c in _pressedChars)
        {
            io.AddInputCharacter(c);
        }

        _pressedChars.Clear();

        io.KeyCtrl = keyboardState.IsKeyPressed(Key.ControlLeft) || keyboardState.IsKeyPressed(Key.ControlRight);
        io.KeyAlt = keyboardState.IsKeyPressed(Key.AltLeft) || keyboardState.IsKeyPressed(Key.AltRight);
        io.KeyShift = keyboardState.IsKeyPressed(Key.ShiftLeft) || keyboardState.IsKeyPressed(Key.ShiftRight);
        io.KeySuper = keyboardState.IsKeyPressed(Key.SuperLeft) || keyboardState.IsKeyPressed(Key.SuperRight);
    }

    internal void PressChar(char keyChar)
    {
        _pressedChars.Add(keyChar);
    }

    private static void SetKeyMappings()
    {
        var io = ImGuiNET.ImGui.GetIO();
        io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
        io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
        io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Backspace;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
        io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
        io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
        io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
        io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
        io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
        io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
    }

    private unsafe void RenderImDrawData(in ImDrawDataPtr drawDataPtr, in CommandBuffer commandBuffer, in Framebuffer framebuffer, in Extent2D swapChainExtent)
    {
        int framebufferWidth = (int)(drawDataPtr.DisplaySize.X * drawDataPtr.FramebufferScale.X);
        int framebufferHeight = (int)(drawDataPtr.DisplaySize.Y * drawDataPtr.FramebufferScale.Y);
        if (framebufferWidth <= 0 || framebufferHeight <= 0)
        {
            return;
        }

        var renderPassInfo = new RenderPassBeginInfo();
        renderPassInfo.SType = StructureType.RenderPassBeginInfo;
        renderPassInfo.RenderPass = _renderPass;
        renderPassInfo.Framebuffer = framebuffer;
        renderPassInfo.RenderArea.Offset = default;
        renderPassInfo.RenderArea.Extent = swapChainExtent;
        renderPassInfo.ClearValueCount = 0;
        renderPassInfo.PClearValues = default;

        vk.CmdBeginRenderPass(commandBuffer, &renderPassInfo, SubpassContents.Inline);

        var drawData = *drawDataPtr.NativePtr;

        // Avoid rendering when minimized, scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
        int fb_width = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        int fb_height = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
        if (fb_width <= 0 || fb_height <= 0)
        {
            return;
        }

        // Allocate array to store enough vertex/index buffers
        if (_mainWindowRenderBuffers.FrameRenderBuffers == null)
        {
            _mainWindowRenderBuffers.Index = 0;
            _mainWindowRenderBuffers.Count = (uint)swapChainImageCt;
            _frameRenderBuffers = GlobalMemory.Allocate(sizeof(FrameRenderBuffer) * (int)_mainWindowRenderBuffers.Count);
            _mainWindowRenderBuffers.FrameRenderBuffers = _frameRenderBuffers.AsPtr<FrameRenderBuffer>();
            for (int i = 0; i < (int)_mainWindowRenderBuffers.Count; i++)
            {
                _mainWindowRenderBuffers.FrameRenderBuffers[i].IndexBuffer.Handle = 0;
                _mainWindowRenderBuffers.FrameRenderBuffers[i].IndexBufferSize = 0;
                _mainWindowRenderBuffers.FrameRenderBuffers[i].IndexBufferMemory.Handle = 0;
                _mainWindowRenderBuffers.FrameRenderBuffers[i].VertexBuffer.Handle = 0;
                _mainWindowRenderBuffers.FrameRenderBuffers[i].VertexBufferSize = 0;
                _mainWindowRenderBuffers.FrameRenderBuffers[i].VertexBufferMemory.Handle = 0;
            }
        }
        _mainWindowRenderBuffers.Index = (_mainWindowRenderBuffers.Index + 1) % _mainWindowRenderBuffers.Count;

        ref FrameRenderBuffer frameRenderBuffer = ref _mainWindowRenderBuffers.FrameRenderBuffers[_mainWindowRenderBuffers.Index];

        if (drawData.TotalVtxCount > 0)
        {
            // Create or resize the vertex/index buffers
            ulong vertex_size = (ulong)drawData.TotalVtxCount * (ulong)sizeof(ImDrawVert);
            ulong index_size = (ulong)drawData.TotalIdxCount * (ulong)sizeof(ushort);
            if (frameRenderBuffer.VertexBuffer.Handle == default || frameRenderBuffer.VertexBufferSize < vertex_size)
            {
                CreateOrResizeBuffer(ref frameRenderBuffer.VertexBuffer, ref frameRenderBuffer.VertexBufferMemory, ref frameRenderBuffer.VertexBufferSize, vertex_size, BufferUsageFlags.VertexBufferBit);
            }
            if (frameRenderBuffer.IndexBuffer.Handle == default || frameRenderBuffer.IndexBufferSize < index_size)
            {
                CreateOrResizeBuffer(ref frameRenderBuffer.IndexBuffer, ref frameRenderBuffer.IndexBufferMemory, ref frameRenderBuffer.IndexBufferSize, index_size, BufferUsageFlags.IndexBufferBit);
            }

            // Upload vertex/index data into a single contiguous GPU buffer
            ImDrawVert* vtx_dst = null;
            ushort* idx_dst = null;
            if (vk.MapMemory(device.VkDevice, frameRenderBuffer.VertexBufferMemory, 0, frameRenderBuffer.VertexBufferSize, 0, (void**)(&vtx_dst)) != Result.Success)
            {
                throw new Exception($"Unable to map device memory");
            }
            if (vk.MapMemory(device.VkDevice, frameRenderBuffer.IndexBufferMemory, 0, frameRenderBuffer.IndexBufferSize, 0, (void**)(&idx_dst)) != Result.Success)
            {
                throw new Exception($"Unable to map device memory");
            }
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawList* cmd_list = drawData.CmdLists[n];
                Unsafe.CopyBlock(vtx_dst, cmd_list->VtxBuffer.Data.ToPointer(), (uint)cmd_list->VtxBuffer.Size * (uint)sizeof(ImDrawVert));
                Unsafe.CopyBlock(idx_dst, cmd_list->IdxBuffer.Data.ToPointer(), (uint)cmd_list->IdxBuffer.Size * (uint)sizeof(ushort));
                vtx_dst += cmd_list->VtxBuffer.Size;
                idx_dst += cmd_list->IdxBuffer.Size;
            }

            Span<MappedMemoryRange> range = stackalloc MappedMemoryRange[2];
            range[0].SType = StructureType.MappedMemoryRange;
            range[0].Memory = frameRenderBuffer.VertexBufferMemory;
            range[0].Size = Vk.WholeSize;
            range[1].SType = StructureType.MappedMemoryRange;
            range[1].Memory = frameRenderBuffer.IndexBufferMemory;
            range[1].Size = Vk.WholeSize;
            if (vk.FlushMappedMemoryRanges(device.VkDevice, 2, range) != Result.Success)
            {
                throw new Exception($"Unable to flush memory to device");
            }
            vk.UnmapMemory(device.VkDevice, frameRenderBuffer.VertexBufferMemory);
            vk.UnmapMemory(device.VkDevice, frameRenderBuffer.IndexBufferMemory);
        }

        // Setup desired Vulkan state
        vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, pipeline.VkPipeline);
        vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, pipelineLayout, 0, 1, _descriptorSet, 0, null);

        // Bind Vertex And Index Buffer:
        if (drawData.TotalVtxCount > 0)
        {
            ReadOnlySpan<Buffer> vertex_buffers = stackalloc Buffer[] { frameRenderBuffer.VertexBuffer };
            ulong vertex_offset = 0;
            vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertex_buffers, (ulong*)Unsafe.AsPointer(ref vertex_offset));
            vk.CmdBindIndexBuffer(commandBuffer, frameRenderBuffer.IndexBuffer, 0, sizeof(ushort) == 2 ? IndexType.Uint16 : IndexType.Uint32);
        }

        // Setup viewport:
        Viewport viewport;
        viewport.X = 0;
        viewport.Y = 0;
        viewport.Width = (float)fb_width;
        viewport.Height = (float)fb_height;
        viewport.MinDepth = 0.0f;
        viewport.MaxDepth = 1.0f;
        vk.CmdSetViewport(commandBuffer, 0, 1, &viewport);

        // Setup scale and translation:
        // Our visible imgui space lies from draw_data.DisplayPps (top left) to draw_data.DisplayPos+data_data.DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
        Span<float> scale = stackalloc float[2];
        scale[0] = 2.0f / drawData.DisplaySize.X;
        scale[1] = 2.0f / drawData.DisplaySize.Y;
        Span<float> translate = stackalloc float[2];
        translate[0] = -1.0f - drawData.DisplayPos.X * scale[0];
        translate[1] = -1.0f - drawData.DisplayPos.Y * scale[1];
        vk.CmdPushConstants(commandBuffer, pipelineLayout, ShaderStageFlags.VertexBit, sizeof(float) * 0, sizeof(float) * 2, scale);
        vk.CmdPushConstants(commandBuffer, pipelineLayout, ShaderStageFlags.VertexBit, sizeof(float) * 2, sizeof(float) * 2, translate);

        // Will project scissor/clipping rectangles into framebuffer space
        Vector2 clipOff = drawData.DisplayPos;         // (0,0) unless using multi-viewports
        Vector2 clipScale = drawData.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

        // Render command lists
        // (Because we merged all buffers into a single one, we maintain our own offset into them)
        int vertexOffset = 0;
        int indexOffset = 0;
        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ref ImDrawList* cmd_list = ref drawData.CmdLists[n];
            for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
            {
                ref ImDrawCmd pcmd = ref cmd_list->CmdBuffer.Ref<ImDrawCmd>(cmd_i);

                // Project scissor/clipping rectangles into framebuffer space
                Vector4 clipRect;
                clipRect.X = (pcmd.ClipRect.X - clipOff.X) * clipScale.X;
                clipRect.Y = (pcmd.ClipRect.Y - clipOff.Y) * clipScale.Y;
                clipRect.Z = (pcmd.ClipRect.Z - clipOff.X) * clipScale.X;
                clipRect.W = (pcmd.ClipRect.W - clipOff.Y) * clipScale.Y;

                if (clipRect.X < fb_width && clipRect.Y < fb_height && clipRect.Z >= 0.0f && clipRect.W >= 0.0f)
                {
                    // Negative offsets are illegal for vkCmdSetScissor
                    if (clipRect.X < 0.0f)
                        clipRect.X = 0.0f;
                    if (clipRect.Y < 0.0f)
                        clipRect.Y = 0.0f;

                    // Apply scissor/clipping rectangle
                    Rect2D scissor = new Rect2D();
                    scissor.Offset.X = (int)clipRect.X;
                    scissor.Offset.Y = (int)clipRect.Y;
                    scissor.Extent.Width = (uint)(clipRect.Z - clipRect.X);
                    scissor.Extent.Height = (uint)(clipRect.W - clipRect.Y);
                    vk.CmdSetScissor(commandBuffer, 0, 1, &scissor);

                    // Draw
                    vk.CmdDrawIndexed(commandBuffer, pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)indexOffset, (int)pcmd.VtxOffset + vertexOffset, 0);
                }
            }
            indexOffset += cmd_list->IdxBuffer.Size;
            vertexOffset += cmd_list->VtxBuffer.Size;
        }

        vk.CmdEndRenderPass(commandBuffer);
    }


    unsafe void CreateOrResizeBuffer(ref Buffer buffer, ref DeviceMemory buffer_memory, ref ulong bufferSize, ulong newSize, BufferUsageFlags usage)
    {
        if (buffer.Handle != default)
        {
            vk.DestroyBuffer(device.VkDevice, buffer, default);
        }
        if (buffer_memory.Handle != default)
        {
            vk.FreeMemory(device.VkDevice, buffer_memory, default);
        }

        ulong sizeAlignedVertexBuffer = ((newSize - 1) / _bufferMemoryAlignment + 1) * _bufferMemoryAlignment;
        var bufferInfo = new BufferCreateInfo();
        bufferInfo.SType = StructureType.BufferCreateInfo;
        bufferInfo.Size = sizeAlignedVertexBuffer;
        bufferInfo.Usage = usage;
        bufferInfo.SharingMode = SharingMode.Exclusive;
        if (vk.CreateBuffer(device.VkDevice, bufferInfo, default, out buffer) != Result.Success)
        {
            throw new Exception($"Unable to create a device buffer");
        }

        vk.GetBufferMemoryRequirements(device.VkDevice, buffer, out var req);
        _bufferMemoryAlignment = (_bufferMemoryAlignment > req.Alignment) ? _bufferMemoryAlignment : req.Alignment;
        MemoryAllocateInfo allocInfo = new MemoryAllocateInfo();
        allocInfo.SType = StructureType.MemoryAllocateInfo;
        allocInfo.AllocationSize = req.Size;
        allocInfo.MemoryTypeIndex = GetMemoryTypeIndex(vk, MemoryPropertyFlags.HostVisibleBit, req.MemoryTypeBits);
        if (vk.AllocateMemory(device.VkDevice, &allocInfo, default, out buffer_memory) != Result.Success)
        {
            throw new Exception($"Unable to allocate device memory");
        }

        if (vk.BindBufferMemory(device.VkDevice, buffer, buffer_memory, 0) != Result.Success)
        {
            throw new Exception($"Unable to bind device memory");
        }
        bufferSize = req.Size;
    }

    struct FrameRenderBuffer
    {
        public DeviceMemory VertexBufferMemory;
        public DeviceMemory IndexBufferMemory;
        public ulong VertexBufferSize;
        public ulong IndexBufferSize;
        public Buffer VertexBuffer;
        public Buffer IndexBuffer;
    };

    unsafe struct WindowRenderBuffers
    {
        public uint Index;
        public uint Count;
        public FrameRenderBuffer* FrameRenderBuffers;
    };


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
    // ~ImGuiRenderSystem()
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
}
