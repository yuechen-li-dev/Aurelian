using Aurelian.Graphics.Plants;
using Aurelian.Graphics.Vulkan.Device;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Aurelian.Graphics.Vulkan.Presentation;

public sealed class AurelianVulkanSurface : IDisposable
{
    private readonly AurelianVulkanPlant plant;
    private readonly KhrSurface surfaceApi;
    private IWindow? window;
    private SurfaceKHR surface;
    private bool disposed;

    internal AurelianVulkanSurface(
        AurelianVulkanPlant plant,
        KhrSurface surfaceApi,
        IWindow window,
        SurfaceKHR surface,
        VulkanSurfaceFacts facts)
    {
        this.plant = plant;
        this.surfaceApi = surfaceApi;
        this.window = window;
        this.surface = surface;
        Facts = facts;
    }

    public PlantId PlantId => plant.Context.Id;

    public uint Width => Facts.Width;

    public uint Height => Facts.Height;

    public VulkanSurfaceFacts Facts { get; }

    internal SurfaceKHR Handle => surface;

    /// <summary>
    /// Pumps the owned Silk.NET window once so visible sample windows can process platform events.
    /// This is intentionally minimal M0 presentation support, not an engine frame loop or input abstraction.
    /// </summary>
    public void PumpEvents()
    {
        if (disposed)
        {
            return;
        }

        window?.DoEvents();
    }

    public unsafe void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (surface.Handle != 0 && plant.Instance.Handle != 0)
        {
            surfaceApi.DestroySurface(plant.Instance, surface, (AllocationCallbacks*)null);
            surface = default;
        }

        window?.Dispose();
        window = null;
        surfaceApi.Dispose();
    }
}
