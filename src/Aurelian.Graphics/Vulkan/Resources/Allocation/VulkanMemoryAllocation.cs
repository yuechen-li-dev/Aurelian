using Aurelian.Graphics.Plants;
using Silk.NET.Vulkan;

namespace Aurelian.Graphics.Vulkan.Resources.Allocation;

public sealed class VulkanMemoryAllocation : IDisposable
{
    private readonly Action<VulkanMemoryAllocation> free;
    private bool disposed;

    internal VulkanMemoryAllocation(
        PlantId plantId,
        VulkanAllocationBackendKind backend,
        DeviceMemory memory,
        ulong offset,
        ulong sizeBytes,
        VulkanMemoryUsage usage,
        Action<VulkanMemoryAllocation> free)
    {
        PlantId = plantId;
        Backend = backend;
        Memory = memory;
        Offset = offset;
        SizeBytes = sizeBytes;
        Usage = usage;
        this.free = free;
    }

    public PlantId PlantId { get; }

    public VulkanAllocationBackendKind Backend { get; }

    public DeviceMemory Memory { get; }

    public ulong Offset { get; }

    public ulong SizeBytes { get; }

    public VulkanMemoryUsage Usage { get; }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        free(this);
    }
}
