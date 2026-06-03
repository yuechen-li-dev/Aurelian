using Aurelian.Graphics.Plants;
using Silk.NET.Vulkan;

namespace Aurelian.Graphics.Vulkan.Commanding;

public sealed class VulkanCommandBufferLease
{
    private readonly object gate = new();
    private readonly Vk vk;
    private VulkanCommandBufferLifecycle lifecycle = VulkanCommandBufferLifecycle.Ready;

    internal VulkanCommandBufferLease(
        PlantId plantId,
        Vk vk,
        CommandBuffer commandBuffer)
    {
        PlantId = plantId;
        this.vk = vk;
        CommandBuffer = commandBuffer;
    }

    public PlantId PlantId { get; }

    public CommandBuffer CommandBuffer { get; }

    public bool IsReady
    {
        get
        {
            lock (gate)
            {
                return lifecycle == VulkanCommandBufferLifecycle.Ready;
            }
        }
    }

    public bool IsRecording
    {
        get
        {
            lock (gate)
            {
                return lifecycle == VulkanCommandBufferLifecycle.Recording;
            }
        }
    }

    public bool IsExecutable
    {
        get
        {
            lock (gate)
            {
                return lifecycle == VulkanCommandBufferLifecycle.Executable;
            }
        }
    }

    public bool IsRetired
    {
        get
        {
            lock (gate)
            {
                return lifecycle == VulkanCommandBufferLifecycle.Retired;
            }
        }
    }

    public VulkanCommandBufferOperationResult Reset()
    {
        lock (gate)
        {
            if (lifecycle == VulkanCommandBufferLifecycle.Disposed)
            {
                return Failed(
                    VulkanCommandBufferDiagnosticCodes.CommandBufferDisposed,
                    "Cannot reset a disposed Vulkan command buffer lease.");
            }
        }

        Result result = vk.ResetCommandBuffer(CommandBuffer, CommandBufferResetFlags.None);
        if (result != Result.Success)
        {
            return Failed(
                VulkanCommandBufferDiagnosticCodes.CommandBufferResetFailed,
                $"Vulkan command buffer reset failed with result {result}.");
        }

        lock (gate)
        {
            if (lifecycle != VulkanCommandBufferLifecycle.Disposed)
            {
                lifecycle = VulkanCommandBufferLifecycle.Ready;
            }
        }

        return VulkanCommandBufferOperationResult.Succeeded();
    }

    public unsafe VulkanCommandBufferOperationResult Begin()
    {
        lock (gate)
        {
            if (lifecycle == VulkanCommandBufferLifecycle.Disposed)
            {
                return Failed(
                    VulkanCommandBufferDiagnosticCodes.CommandBufferDisposed,
                    "Cannot begin recording a disposed Vulkan command buffer lease.");
            }

            if (lifecycle != VulkanCommandBufferLifecycle.Ready)
            {
                return Failed(
                    VulkanCommandBufferDiagnosticCodes.InvalidCommandBufferState,
                    $"Cannot begin a Vulkan command buffer while it is {lifecycle}; expected Ready.");
            }
        }

        CommandBufferBeginInfo beginInfo = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };

        Result result = vk.BeginCommandBuffer(CommandBuffer, &beginInfo);
        if (result != Result.Success)
        {
            return Failed(
                VulkanCommandBufferDiagnosticCodes.CommandBufferBeginFailed,
                $"Vulkan command buffer begin failed with result {result}.");
        }

        lock (gate)
        {
            if (lifecycle != VulkanCommandBufferLifecycle.Disposed)
            {
                lifecycle = VulkanCommandBufferLifecycle.Recording;
            }
        }

        return VulkanCommandBufferOperationResult.Succeeded();
    }

    public VulkanCommandBufferOperationResult End()
    {
        lock (gate)
        {
            if (lifecycle == VulkanCommandBufferLifecycle.Disposed)
            {
                return Failed(
                    VulkanCommandBufferDiagnosticCodes.CommandBufferDisposed,
                    "Cannot end recording a disposed Vulkan command buffer lease.");
            }

            if (lifecycle != VulkanCommandBufferLifecycle.Recording)
            {
                return Failed(
                    VulkanCommandBufferDiagnosticCodes.InvalidCommandBufferState,
                    $"Cannot end a Vulkan command buffer while it is {lifecycle}; expected Recording.");
            }
        }

        Result result = vk.EndCommandBuffer(CommandBuffer);
        if (result != Result.Success)
        {
            return Failed(
                VulkanCommandBufferDiagnosticCodes.CommandBufferEndFailed,
                $"Vulkan command buffer end failed with result {result}.");
        }

        lock (gate)
        {
            if (lifecycle != VulkanCommandBufferLifecycle.Disposed)
            {
                lifecycle = VulkanCommandBufferLifecycle.Executable;
            }
        }

        return VulkanCommandBufferOperationResult.Succeeded();
    }

    internal void MarkRetired()
    {
        lock (gate)
        {
            if (lifecycle != VulkanCommandBufferLifecycle.Disposed)
            {
                lifecycle = VulkanCommandBufferLifecycle.Retired;
            }
        }
    }

    internal void MarkDisposed()
    {
        lock (gate)
        {
            lifecycle = VulkanCommandBufferLifecycle.Disposed;
        }
    }

    private VulkanCommandBufferOperationResult Failed(string code, string message)
        => VulkanCommandBufferOperationResult.Failed(new VulkanCommandBufferDiagnostic(
            code,
            VulkanCommandBufferDiagnosticSeverity.Error,
            message,
            PlantId));
}
