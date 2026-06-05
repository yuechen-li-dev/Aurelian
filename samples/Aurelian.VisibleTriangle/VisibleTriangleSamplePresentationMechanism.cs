using Aurelian.Core.Engine.Graphics;
using Aurelian.Graphics.Vulkan.Presentation;

namespace Aurelian.VisibleTriangle;

internal sealed class VisibleTriangleSamplePresentationMechanism : IPresentationMechanism
{
    private readonly AurelianVulkanSwapchain swapchain;
    private readonly uint imageIndex;

    public VisibleTriangleSamplePresentationMechanism(AurelianVulkanSwapchain swapchain, uint imageIndex)
    {
        ArgumentNullException.ThrowIfNull(swapchain);
        this.swapchain = swapchain;
        this.imageIndex = imageIndex;
    }

    public VulkanSwapchainPresentResult Present() => swapchain.Present(imageIndex);

    public Task PresentAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        VulkanSwapchainPresentResult result = Present();
        return result.Status is VulkanSwapchainPresentStatus.Presented or VulkanSwapchainPresentStatus.Suboptimal or VulkanSwapchainPresentStatus.OutOfDate
            ? Task.CompletedTask
            : Task.FromException(new InvalidOperationException($"Swapchain present failed with status {result.Status}: {FormatDiagnostics(result)}"));
    }

    private static string FormatDiagnostics(VulkanSwapchainPresentResult result) =>
        string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));
}
