using Aurelian.Graphics.Plants;
using Aurelian.Graphics.Vulkan.Device;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Aurelian.Graphics.Vulkan.Presentation;

public sealed class AurelianVulkanSwapchain : IDisposable
{
    private readonly AurelianVulkanPlant plant;
    private readonly KhrSwapchain swapchainApi;
    private readonly ImageView[] imageViews;
    private SwapchainKHR swapchain;
    private bool disposed;

    internal AurelianVulkanSwapchain(
        AurelianVulkanPlant plant,
        KhrSwapchain swapchainApi,
        SwapchainKHR swapchain,
        IReadOnlyList<Image> images,
        ImageView[] imageViews,
        VulkanSwapchainFacts facts)
    {
        this.plant = plant;
        this.swapchainApi = swapchainApi;
        this.swapchain = swapchain;
        this.imageViews = imageViews;
        Images = images.ToArray();
        ImageViews = imageViews.ToArray();
        Facts = facts;
    }

    public PlantId PlantId => plant.Context.Id;

    public VulkanSwapchainFacts Facts { get; }

    public IReadOnlyList<Image> Images { get; }

    public IReadOnlyList<ImageView> ImageViews { get; }

    public VulkanSwapchainAcquireResult AcquireNextImage()
        => new(
            VulkanPresentationStatus.Rejected,
            null,
            [new VulkanPresentationDiagnostic(
                VulkanPresentationDiagnosticCodes.AcquirePresentDeferred,
                VulkanPresentationDiagnosticSeverity.Info,
                "Swapchain acquire is deferred to the acquire/present milestone because A48 intentionally avoids binary semaphore ownership and a present loop.",
                PlantId)]);

    public VulkanPresentationResult Present(uint imageIndex)
        => new(
            VulkanPresentationStatus.Rejected,
            [new VulkanPresentationDiagnostic(
                VulkanPresentationDiagnosticCodes.AcquirePresentDeferred,
                VulkanPresentationDiagnosticSeverity.Info,
                $"Swapchain present for image {imageIndex} is deferred to the acquire/present milestone because A48 intentionally avoids binary semaphore ownership and a present loop.",
                PlantId)]);

    public unsafe void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        foreach (ImageView imageView in imageViews)
        {
            if (imageView.Handle != 0 && plant.Device.Handle != 0)
            {
                plant.Vk.DestroyImageView(plant.Device, imageView, (AllocationCallbacks*)null);
            }
        }

        if (swapchain.Handle != 0 && plant.Device.Handle != 0)
        {
            swapchainApi.DestroySwapchain(plant.Device, swapchain, (AllocationCallbacks*)null);
            swapchain = default;
        }

        swapchainApi.Dispose();
    }
}
