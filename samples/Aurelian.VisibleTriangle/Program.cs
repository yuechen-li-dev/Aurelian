using Aurelian.Core.Engine;
using Aurelian.Core.Engine.Frames;
using Aurelian.Graphics.Vulkan.Presentation;
using Aurelian.Rendering.Contracts.Compositor;
using Aurelian.Runtime.Compositor;

namespace Aurelian.VisibleTriangle;

internal static class Program
{
    private const int Success = 0;
    private const int EnvironmentOrRuntimeFailure = 2;
    private const int UnexpectedFailure = 3;

    public static async Task<int> Main(string[] args)
    {
        bool enableValidation = args.Contains("--validation", StringComparer.OrdinalIgnoreCase);
        bool skipHold = args.Contains("--no-hold", StringComparer.OrdinalIgnoreCase);

        Console.WriteLine("Aurelian A62 visible triangle sample");
        Console.WriteLine("Path: core frame pump -> runtime compositor policy -> core compositor bridge -> Vulkan compositor -> offscreen triangle -> swapchain present");
        Console.WriteLine($"Validation: {(enableValidation ? "enabled" : "disabled")}");

        try
        {
            using VisibleTriangleSampleFrame sample = VisibleTriangleSampleFrame.Create(enableValidation);
            Console.WriteLine($"Prepared visible Vulkan setup created ({sample.SwapchainDescription}); acquired image {sample.AcquiredImageIndex}.");
            Console.WriteLine($"Engine status after start: {sample.Engine.Status}; graphics mode: {sample.Engine.Options.Graphics.Mode} ({sample.Engine.Options.Graphics.Ownership}).");

            AurelianFrameResult frameResult = await sample.FramePump.RunOneFrameAsync(sample.Input).ConfigureAwait(false);
            if (!frameResult.Success)
            {
                Console.Error.WriteLine($"Frame pump failed with status {frameResult.Status}:");
                Console.Error.WriteLine(FormatDiagnostics(frameResult));
                return EnvironmentOrRuntimeFailure;
            }

            Console.WriteLine($"Frame pump completed frame {frameResult.FrameId.Value} with compositor status {frameResult.CompositorResult?.Status}.");
            Console.WriteLine($"Compositor dispatch status: {frameResult.CompositorResult?.DispatchResult?.Status}; target: {FormatTarget(frameResult.CompositorResult?.DispatchResult?.Target)}.");

            await sample.PresentationMechanism.PresentAsync().ConfigureAwait(false);
            Console.WriteLine("Presented compositor output to the swapchain.");

            if (!skipHold)
            {
                Console.WriteLine("Keeping the window responsive briefly (pass --no-hold to exit immediately). ");
                DateTimeOffset end = DateTimeOffset.UtcNow.AddSeconds(2);
                while (DateTimeOffset.UtcNow < end)
                {
                    sample.PumpEvents();
                    await Task.Delay(16).ConfigureAwait(false);
                }
            }

            AurelianEngineResult stop = sample.Engine.Stop();
            if (!stop.Success)
            {
                Console.Error.WriteLine($"Engine stop returned status {stop.Status}: {FormatDiagnostics(stop)}");
                return EnvironmentOrRuntimeFailure;
            }

            Console.WriteLine("A62 visible triangle sample completed successfully.");
            return Success;
        }
        catch (VisibleTriangleSampleException ex)
        {
            Console.Error.WriteLine("A62 visible triangle sample could not run in this environment or failed during the Vulkan sample path.");
            Console.Error.WriteLine(ex.Message);
            return EnvironmentOrRuntimeFailure;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("A62 visible triangle sample hit an unexpected exception.");
            Console.Error.WriteLine(ex);
            return UnexpectedFailure;
        }
    }

    private static string FormatDiagnostics(AurelianFrameResult result) =>
        string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));

    private static string FormatDiagnostics(AurelianEngineResult result) =>
        string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));

    private static string FormatTarget(PresentationTargetRef? target)
    {
        if (target is null)
        {
            return "<none>";
        }

        PresentationTargetRef value = target.Value;
        return $"plant={value.PlantId}, image={value.SwapchainImageIndex}, frame={value.FrameId}";
    }
}
