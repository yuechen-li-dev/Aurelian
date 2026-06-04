using Aurelian.Graphics.Plants;
using Aurelian.Graphics.Vulkan.Device;
using Silk.NET.Vulkan;

namespace Aurelian.Graphics.Vulkan.Resources.Allocation;

public sealed unsafe class RawVulkanMemoryAllocator : IVulkanMemoryAllocator
{
    private readonly object gate = new();
    private readonly Vk vk;
    private readonly Silk.NET.Vulkan.Device device;
    private readonly PhysicalDeviceMemoryProperties memoryProperties;
    private ulong allocationCount;
    private ulong freeCount;
    private ulong liveAllocationCount;
    private ulong requestedBytes;
    private ulong liveBytes;
    private ulong highWaterLiveBytes;
    private bool disposed;

    public RawVulkanMemoryAllocator(AurelianVulkanPlant plant)
    {
        ArgumentNullException.ThrowIfNull(plant);
        vk = plant.Vk;
        device = plant.Device;
        PlantId = plant.Context.Id;
        vk.GetPhysicalDeviceMemoryProperties(plant.PhysicalDevice, out memoryProperties);
    }

    public PlantId PlantId { get; }

    public VulkanMemoryAllocatorTelemetry Telemetry
    {
        get
        {
            lock (gate)
            {
                return SnapshotTelemetry();
            }
        }
    }

    public VulkanAllocationResult Allocate(VulkanAllocationRequest request)
    {
        List<VulkanMemoryAllocatorDiagnostic> diagnostics = [];
        lock (gate)
        {
            if (disposed)
            {
                diagnostics.Add(Diagnostic(
                    VulkanMemoryAllocatorDiagnosticCodes.AllocatorDisposed,
                    VulkanMemoryAllocatorDiagnosticSeverity.Error,
                    "The Vulkan memory allocator has been disposed.",
                    request.PlantId,
                    request.DebugName));
                return new VulkanAllocationResult(VulkanMemoryAllocatorStatus.Rejected, null, diagnostics);
            }

            if (request.SizeBytes == 0)
            {
                diagnostics.Add(Diagnostic(
                    VulkanMemoryAllocatorDiagnosticCodes.InvalidAllocationSize,
                    VulkanMemoryAllocatorDiagnosticSeverity.Error,
                    "Allocation size must be greater than zero bytes.",
                    request.PlantId,
                    request.DebugName));
                return new VulkanAllocationResult(VulkanMemoryAllocatorStatus.Rejected, null, diagnostics);
            }

            if (request.MemoryTypeBits == 0)
            {
                diagnostics.Add(Diagnostic(
                    VulkanMemoryAllocatorDiagnosticCodes.InvalidMemoryTypeBits,
                    VulkanMemoryAllocatorDiagnosticSeverity.Error,
                    "Memory type bits must include at least one compatible memory type.",
                    request.PlantId,
                    request.DebugName));
                return new VulkanAllocationResult(VulkanMemoryAllocatorStatus.Rejected, null, diagnostics);
            }

            if (request.PlantId != PlantId)
            {
                diagnostics.Add(Diagnostic(
                    VulkanMemoryAllocatorDiagnosticCodes.PlantMismatch,
                    VulkanMemoryAllocatorDiagnosticSeverity.Error,
                    "Allocation request plant does not match the allocator plant.",
                    request.PlantId,
                    request.DebugName));
                return new VulkanAllocationResult(VulkanMemoryAllocatorStatus.Rejected, null, diagnostics);
            }

            if (!TryFindMemoryTypeIndex(request.MemoryTypeBits, request.Usage, out uint memoryTypeIndex))
            {
                diagnostics.Add(Diagnostic(
                    VulkanMemoryAllocatorDiagnosticCodes.NoSuitableMemoryType,
                    VulkanMemoryAllocatorDiagnosticSeverity.Error,
                    "No compatible Vulkan memory type matched the requested usage flags.",
                    request.PlantId,
                    request.DebugName));
                return new VulkanAllocationResult(VulkanMemoryAllocatorStatus.Rejected, null, diagnostics);
            }

            MemoryAllocateInfo allocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = request.SizeBytes,
                MemoryTypeIndex = memoryTypeIndex,
            };

            Result result;
            DeviceMemory memory;
            result = vk.AllocateMemory(device, &allocateInfo, (AllocationCallbacks*)null, out memory);

            if (result != Result.Success)
            {
                diagnostics.Add(Diagnostic(
                    VulkanMemoryAllocatorDiagnosticCodes.AllocationFailed,
                    VulkanMemoryAllocatorDiagnosticSeverity.Error,
                    $"vkAllocateMemory failed with result {result}.",
                    request.PlantId,
                    request.DebugName));
                return new VulkanAllocationResult(VulkanMemoryAllocatorStatus.Failed, null, diagnostics);
            }

            allocationCount++;
            liveAllocationCount++;
            requestedBytes += request.SizeBytes;
            liveBytes += request.SizeBytes;
            highWaterLiveBytes = Math.Max(highWaterLiveBytes, liveBytes);

            VulkanMemoryAllocation allocation = new(
                PlantId,
                VulkanAllocationBackendKind.RawVulkan,
                memory,
                0,
                request.SizeBytes,
                request.Usage,
                FreeAllocation);

            return new VulkanAllocationResult(VulkanMemoryAllocatorStatus.Allocated, allocation, diagnostics);
        }
    }

    public bool TryFindMemoryTypeIndex(uint memoryTypeBits, VulkanMemoryUsage usage, out uint index)
    {
        MemoryPropertyFlags requiredFlags = RequiredFlags(usage);
        for (uint i = 0; i < memoryProperties.MemoryTypeCount; i++)
        {
            uint bit = 1u << (int)i;
            if ((memoryTypeBits & bit) == 0)
            {
                continue;
            }

            MemoryType memoryType = memoryProperties.MemoryTypes[(int)i];
            if ((memoryType.PropertyFlags & requiredFlags) == requiredFlags)
            {
                index = i;
                return true;
            }
        }

        index = 0;
        return false;
    }

    public void Dispose()
    {
        lock (gate)
        {
            disposed = true;
        }
    }

    private void FreeAllocation(VulkanMemoryAllocation allocation)
    {
        lock (gate)
        {
            if (allocation.PlantId != PlantId || allocation.Backend != VulkanAllocationBackendKind.RawVulkan)
            {
                return;
            }

            if (allocation.Memory.Handle != 0)
            {
                vk.FreeMemory(device, allocation.Memory, (AllocationCallbacks*)null);
            }

            freeCount++;
            if (liveAllocationCount > 0)
            {
                liveAllocationCount--;
            }

            liveBytes = allocation.SizeBytes > liveBytes ? 0 : liveBytes - allocation.SizeBytes;
        }
    }

    private VulkanMemoryAllocatorTelemetry SnapshotTelemetry()
        => new(
            PlantId,
            VulkanAllocationBackendKind.RawVulkan,
            allocationCount,
            freeCount,
            liveAllocationCount,
            requestedBytes,
            liveBytes,
            highWaterLiveBytes);

    private static MemoryPropertyFlags RequiredFlags(VulkanMemoryUsage usage)
        => usage switch
        {
            VulkanMemoryUsage.GpuOnly => MemoryPropertyFlags.DeviceLocalBit,
            VulkanMemoryUsage.CpuToGpu => MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            VulkanMemoryUsage.GpuToCpu => MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            _ => 0,
        };

    private static VulkanMemoryAllocatorDiagnostic Diagnostic(
        string code,
        VulkanMemoryAllocatorDiagnosticSeverity severity,
        string message,
        PlantId plantId,
        string? debugName)
        => new(code, severity, message, plantId, debugName);
}
