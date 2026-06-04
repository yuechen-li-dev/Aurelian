namespace Aurelian.Graphics.Vulkan.Presentation;

public sealed record VulkanSwapchainAcquireResult(
    VulkanPresentationStatus Status,
    uint? ImageIndex,
    IReadOnlyList<VulkanPresentationDiagnostic> Diagnostics)
{
    public bool Success => Status == VulkanPresentationStatus.Acquired && ImageIndex is not null;
}
