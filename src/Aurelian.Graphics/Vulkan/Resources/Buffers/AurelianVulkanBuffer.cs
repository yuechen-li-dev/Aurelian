using Aurelian.Graphics.Plants;
using Aurelian.Graphics.Vulkan.Resources.Allocation;
using Silk.NET.Vulkan;
using NativeBuffer = Silk.NET.Vulkan.Buffer;

namespace Aurelian.Graphics.Vulkan.Resources.Buffers;

public sealed unsafe class AurelianVulkanBuffer : IDisposable
{
    private readonly Vk vk;
    private Silk.NET.Vulkan.Device device;
    private VulkanMemoryAllocation? allocation;
    private bool disposed;

    internal AurelianVulkanBuffer(
        Vk vk,
        Silk.NET.Vulkan.Device device,
        NativeBuffer nativeBuffer,
        VulkanMemoryAllocation allocation,
        PlantId plantId,
        ulong sizeBytes,
        VulkanBufferUsage usage,
        VulkanMemoryUsage memoryUsage)
    {
        this.vk = vk;
        this.device = device;
        NativeBuffer = nativeBuffer;
        this.allocation = allocation;
        PlantId = plantId;
        SizeBytes = sizeBytes;
        Usage = usage;
        MemoryUsage = memoryUsage;
        ResourceState = new GpuResourceState(plantId, sizeBytes, allocation.Backend);
    }

    public PlantId PlantId { get; }

    public ulong SizeBytes { get; }

    public VulkanBufferUsage Usage { get; }

    public VulkanMemoryUsage MemoryUsage { get; }

    public GpuResourceState ResourceState { get; }

    public bool IsDisposed => disposed;

    internal NativeBuffer NativeBuffer { get; private set; }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (NativeBuffer.Handle != 0 && device.Handle != 0)
        {
            vk.DestroyBuffer(device, NativeBuffer, (AllocationCallbacks*)null);
            NativeBuffer = default;
        }

        allocation?.Dispose();
        allocation = null;
        device = default;
    }
}
