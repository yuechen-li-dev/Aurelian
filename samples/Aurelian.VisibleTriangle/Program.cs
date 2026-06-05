using Aurelian.Core.Engine;
using Aurelian.Core.Engine.Frames;
using Aurelian.Core.Engine.Runtime;
using Aurelian.Rendering.Contracts.Compositor;
using Aurelian.Runtime.Sessions;

namespace Aurelian.VisibleTriangle;

internal static class Program
{
    private const int Success = 0;
    private const int EnvironmentOrRuntimeFailure = 2;
    private const int UnexpectedFailure = 3;
    private const int SupportedFrameCount = 1;

    public static async Task<int> Main(string[] args)
    {
        bool enableValidation = args.Contains("--validation", StringComparer.OrdinalIgnoreCase);
        bool skipHold = args.Contains("--no-hold", StringComparer.OrdinalIgnoreCase);
        int frameCount = ParseFrameCount(args);

        Console.WriteLine("Aurelian A66 visible triangle sample");
        Console.WriteLine("Path: prepared Vulkan setup -> AurelianEngine -> AurelianRuntimeSession -> AurelianFrameLoop -> runtime tick -> frame pump -> runtime compositor policy -> core compositor bridge -> Vulkan compositor -> present");
        Console.WriteLine($"Validation: {(enableValidation ? "enabled" : "disabled")}");
        Console.WriteLine($"Requested frames: {frameCount}; supported M0 frames: {SupportedFrameCount}");

        if (frameCount != SupportedFrameCount)
        {
            Console.Error.WriteLine("A66 visible triangle sample currently supports one prepared acquired swapchain image; use --frames 1. Multi-frame acquire/present is deferred.");
            frameCount = SupportedFrameCount;
        }

        VisibleTriangleSampleFrame? sample = null;
        AurelianRuntimeSession? runtimeSession = null;
        bool runtimeStarted = false;

        try
        {
            sample = VisibleTriangleSampleFrame.Create(enableValidation);
            Console.WriteLine($"Prepared visible Vulkan setup created ({sample.SwapchainDescription}); acquired image {sample.AcquiredImageIndex}.");
            Console.WriteLine($"Engine status after start: {sample.Engine.Status}; graphics mode: {sample.Engine.Options.Graphics.Mode} ({sample.Engine.Options.Graphics.Ownership}).");

            runtimeSession = new AurelianRuntimeSession();
            AurelianRuntimeResult runtimeStart = runtimeSession.Start();
            if (!runtimeStart.Success)
            {
                Console.Error.WriteLine("Runtime session start failed:");
                Console.Error.WriteLine(FormatDiagnostics(runtimeStart));
                return EnvironmentOrRuntimeFailure;
            }

            runtimeStarted = true;
            Console.WriteLine("Dominatus-backed runtime session started.");

            var runtimeTicker = new AurelianRuntimeSessionTickerAdapter(runtimeSession);
            var runtimeTickStep = new AurelianRuntimeTickFrameStep(runtimeTicker);
            var inputProvider = new VisibleTriangleFrameInputProvider(sample.Input);
            var frameLoop = new AurelianFrameLoop(
                sample.FramePump,
                inputProvider,
                sample.PresentationMechanism,
                new AurelianFrameLoopOptions(
                    MaxFrames: frameCount,
                    PresentAfterCompletedFrame: true,
                    StopOnFrameFailure: true),
                runtimeTickStep);

            AurelianFrameLoopResult loopResult = await frameLoop.RunAsync(sample.Input.FrameId).ConfigureAwait(false);
            PrintLoopResult(loopResult);

            if (!skipHold && loopResult.Success)
            {
                Console.WriteLine("Keeping the window responsive briefly (pass --no-hold to exit immediately). ");
                DateTimeOffset end = DateTimeOffset.UtcNow.AddSeconds(2);
                while (DateTimeOffset.UtcNow < end)
                {
                    sample.PumpEvents();
                    await Task.Delay(16).ConfigureAwait(false);
                }
            }

            return loopResult.Success ? Success : EnvironmentOrRuntimeFailure;
        }
        catch (VisibleTriangleSampleException ex)
        {
            Console.Error.WriteLine("A66 visible triangle sample could not run in this environment or failed during the Vulkan sample path.");
            Console.Error.WriteLine(ex.Message);
            return EnvironmentOrRuntimeFailure;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("A66 visible triangle sample hit an unexpected exception.");
            Console.Error.WriteLine(ex);
            return UnexpectedFailure;
        }
        finally
        {
            if (runtimeStarted && runtimeSession is not null)
            {
                AurelianRuntimeResult runtimeStop = runtimeSession.Stop();
                if (!runtimeStop.Success)
                {
                    Console.Error.WriteLine("Runtime session stop returned diagnostics:");
                    Console.Error.WriteLine(FormatDiagnostics(runtimeStop));
                }
                else
                {
                    Console.WriteLine("Dominatus-backed runtime session stopped.");
                }
            }

            if (sample is not null)
            {
                if (sample.Engine.Status == AurelianEngineStatus.Started)
                {
                    AurelianEngineResult stop = sample.Engine.Stop();
                    if (!stop.Success)
                    {
                        Console.Error.WriteLine($"Engine stop returned status {stop.Status}: {FormatDiagnostics(stop)}");
                    }
                    else
                    {
                        Console.WriteLine("Aurelian engine stopped.");
                    }
                }

                sample.Dispose();
            }
        }
    }

    private static int ParseFrameCount(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--frames", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out int value) || value <= 0)
            {
                Console.Error.WriteLine("Invalid --frames value; defaulting to 1.");
                return SupportedFrameCount;
            }

            return value;
        }

        return SupportedFrameCount;
    }

    private static void PrintLoopResult(AurelianFrameLoopResult result)
    {
        Console.WriteLine($"Frame loop status: {result.Status}; stop reason: {result.StopReason}; attempted: {result.FramesAttempted}; completed: {result.FramesCompleted}.");

        if (result.Diagnostics.Count > 0)
        {
            Console.WriteLine("Frame loop diagnostics:");
            Console.WriteLine(FormatDiagnostics(result));
        }

        foreach (AurelianFrameLoopIterationResult iteration in result.Iterations)
        {
            Console.WriteLine($"Frame {iteration.FrameId.Value}: frame={iteration.FrameResult.Status}; presented={iteration.Presented}; compositor={iteration.FrameResult.CompositorResult?.Status}.");
            if (iteration.RuntimeTickResult is not null)
            {
                Console.WriteLine($"Frame {iteration.FrameId.Value}: runtime tick={iteration.RuntimeTickResult.Status}; runtime result={iteration.RuntimeTickResult.RuntimeResult?.Status}; tick index={iteration.RuntimeTickResult.RuntimeResult?.TickIndex}.");
                if (iteration.RuntimeTickResult.Diagnostics.Count > 0)
                {
                    Console.WriteLine(FormatDiagnostics(iteration.RuntimeTickResult));
                }
            }

            Console.WriteLine($"Compositor dispatch status: {iteration.FrameResult.CompositorResult?.DispatchResult?.Status}; target: {FormatTarget(iteration.FrameResult.CompositorResult?.DispatchResult?.Target)}.");
            if (iteration.FrameResult.Diagnostics.Count > 0)
            {
                Console.WriteLine(FormatDiagnostics(iteration.FrameResult));
            }
        }
    }

    private static string FormatDiagnostics(AurelianFrameResult result) =>
        string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));

    private static string FormatDiagnostics(AurelianEngineResult result) =>
        string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));

    private static string FormatDiagnostics(AurelianFrameLoopResult result) =>
        string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));

    private static string FormatDiagnostics(AurelianRuntimeResult result) =>
        string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => $"{diagnostic.Code}: {diagnostic.Message}"));

    private static string FormatDiagnostics(AurelianRuntimeTickFrameStepResult result) =>
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
