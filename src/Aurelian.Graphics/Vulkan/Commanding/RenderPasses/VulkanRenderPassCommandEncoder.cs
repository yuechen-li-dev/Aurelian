using Aurelian.Graphics.Plants;
using Aurelian.Graphics.Vulkan.Device;
using Aurelian.Graphics.Vulkan.Pipelines.Framebuffers;
using Aurelian.Graphics.Vulkan.Pipelines.RenderPasses;
using Silk.NET.Vulkan;

namespace Aurelian.Graphics.Vulkan.Commanding.RenderPasses;

public sealed unsafe class VulkanRenderPassCommandEncoder
{
    private CommandBuffer activeCommandBuffer;
    private AurelianVulkanRenderPass? activeRenderPass;
    private AurelianVulkanFramebuffer? activeFramebuffer;

    public VulkanRenderPassCommandResult Begin(
        AurelianVulkanPlant plant,
        VulkanCommandBufferLease commandBuffer,
        VulkanRenderPassBeginRequest request)
    {
        ArgumentNullException.ThrowIfNull(plant);
        ArgumentNullException.ThrowIfNull(commandBuffer);
        ArgumentNullException.ThrowIfNull(request);

        List<VulkanRenderPassCommandDiagnostic> diagnostics = [];
        ValidateBegin(plant, commandBuffer, request, diagnostics);
        if (diagnostics.Any(static diagnostic => diagnostic.Severity == VulkanRenderPassCommandDiagnosticSeverity.Error))
        {
            return new VulkanRenderPassCommandResult(VulkanRenderPassCommandStatus.Rejected, diagnostics);
        }

        try
        {
            ClearValue clearValue = ToNative(request.ClearColor);
            RenderPassBeginInfo beginInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = request.RenderPass.NativeRenderPass,
                Framebuffer = request.Framebuffer.NativeFramebuffer,
                RenderArea = new Rect2D(
                    new Offset2D(0, 0),
                    new Extent2D(request.Framebuffer.Width, request.Framebuffer.Height)),
                ClearValueCount = 1,
                PClearValues = &clearValue,
            };

            plant.Vk.CmdBeginRenderPass(commandBuffer.CommandBuffer, &beginInfo, SubpassContents.Inline);

            activeCommandBuffer = commandBuffer.CommandBuffer;
            activeRenderPass = request.RenderPass;
            activeFramebuffer = request.Framebuffer;
            return VulkanRenderPassCommandResult.Recorded(diagnostics);
        }
        catch (Exception exception)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.BeginRenderPassFailed,
                $"vkCmdBeginRenderPass failed: {exception.Message}",
                plant.Context.Id));
            return new VulkanRenderPassCommandResult(VulkanRenderPassCommandStatus.Failed, diagnostics);
        }
    }

    public VulkanRenderPassCommandResult End(
        AurelianVulkanPlant plant,
        VulkanCommandBufferLease commandBuffer)
    {
        ArgumentNullException.ThrowIfNull(plant);
        ArgumentNullException.ThrowIfNull(commandBuffer);

        List<VulkanRenderPassCommandDiagnostic> diagnostics = [];
        ValidateEnd(plant, commandBuffer, diagnostics);
        if (diagnostics.Any(static diagnostic => diagnostic.Severity == VulkanRenderPassCommandDiagnosticSeverity.Error))
        {
            return new VulkanRenderPassCommandResult(VulkanRenderPassCommandStatus.Rejected, diagnostics);
        }

        try
        {
            plant.Vk.CmdEndRenderPass(commandBuffer.CommandBuffer);
            activeCommandBuffer = default;
            activeRenderPass = null;
            activeFramebuffer = null;
            return VulkanRenderPassCommandResult.Recorded(diagnostics);
        }
        catch (Exception exception)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.EndRenderPassFailed,
                $"vkCmdEndRenderPass failed: {exception.Message}",
                plant.Context.Id));
            return new VulkanRenderPassCommandResult(VulkanRenderPassCommandStatus.Failed, diagnostics);
        }
    }

    private void ValidateBegin(
        AurelianVulkanPlant plant,
        VulkanCommandBufferLease commandBuffer,
        VulkanRenderPassBeginRequest request,
        List<VulkanRenderPassCommandDiagnostic> diagnostics)
    {
        PlantId plantId = plant.Context.Id;
        ValidatePlantAndCommandBuffer(plant, commandBuffer, diagnostics);

        if (activeRenderPass is not null)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.RenderPassAlreadyActive,
                "Cannot begin a render pass while this encoder already has an active render pass.",
                plantId));
        }

        if (request.RenderPass is null)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.RenderPassMissing,
                "Render pass begin requires a render pass.",
                plantId));
        }

        if (request.Framebuffer is null)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.FramebufferMissing,
                "Render pass begin requires a framebuffer.",
                plantId));
        }

        if (request.RenderPass is null || request.Framebuffer is null)
        {
            return;
        }

        if (request.RenderPass.IsDisposed || request.RenderPass.NativeRenderPass.Handle == 0)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.RenderPassDisposed,
                "Cannot begin a render pass with a disposed or empty native render pass.",
                plantId));
        }

        if (request.Framebuffer.IsDisposed || request.Framebuffer.NativeFramebuffer.Handle == 0)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.FramebufferDisposed,
                "Cannot begin a render pass with a disposed or empty native framebuffer.",
                plantId));
        }

        if (request.RenderPass.PlantId != plantId || request.Framebuffer.PlantId != plantId)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.PlantMismatch,
                "Render pass and framebuffer must belong to the target Vulkan plant.",
                plantId));
        }

        if (!ReferenceEquals(request.Framebuffer.RenderPass, request.RenderPass))
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.RenderPassFramebufferMismatch,
                "Framebuffer must have been created for the render pass passed to begin.",
                plantId));
        }
    }

    private void ValidateEnd(
        AurelianVulkanPlant plant,
        VulkanCommandBufferLease commandBuffer,
        List<VulkanRenderPassCommandDiagnostic> diagnostics)
    {
        ValidatePlantAndCommandBuffer(plant, commandBuffer, diagnostics);

        if (activeRenderPass is null)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.NoActiveRenderPass,
                "Cannot end a render pass because this encoder has no active render pass.",
                plant.Context.Id));
            return;
        }

        if (activeCommandBuffer.Handle != commandBuffer.CommandBuffer.Handle)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.RenderPassFramebufferMismatch,
                "Cannot end a render pass from a command buffer other than the one used to begin it.",
                plant.Context.Id));
        }

        if (activeRenderPass.PlantId != plant.Context.Id || activeFramebuffer?.PlantId != plant.Context.Id)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.PlantMismatch,
                "Active render pass state does not belong to the target Vulkan plant.",
                plant.Context.Id));
        }
    }

    private static void ValidatePlantAndCommandBuffer(
        AurelianVulkanPlant plant,
        VulkanCommandBufferLease commandBuffer,
        List<VulkanRenderPassCommandDiagnostic> diagnostics)
    {
        PlantId plantId = plant.Context.Id;
        if (plant.Device.Handle == 0)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.PlantMismatch,
                "Cannot record render pass commands for a disposed or uninitialized Vulkan plant.",
                plantId));
        }

        if (commandBuffer.PlantId != plantId)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.PlantMismatch,
                $"Command buffer plant {commandBuffer.PlantId} does not match Vulkan plant {plantId}.",
                plantId));
        }

        if (!commandBuffer.IsRecording)
        {
            diagnostics.Add(Diagnostic(
                VulkanRenderPassCommandDiagnosticCodes.CommandBufferNotRecording,
                "Render pass commands require a command buffer in Recording state.",
                plantId));
        }
    }

    private static ClearValue ToNative(VulkanColorClearValue clearColor)
        => new()
        {
            Color = new ClearColorValue
            {
                Float32_0 = clearColor.R,
                Float32_1 = clearColor.G,
                Float32_2 = clearColor.B,
                Float32_3 = clearColor.A,
            },
        };

    private static VulkanRenderPassCommandDiagnostic Diagnostic(string code, string message, PlantId plantId)
        => new(code, VulkanRenderPassCommandDiagnosticSeverity.Error, message, plantId);
}
