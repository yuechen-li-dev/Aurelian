# Aurelian Visible Triangle sample

This A66 sample executable demonstrates the current prepared-visible Aurelian engine spine:

```text
sample-owned prepared Vulkan setup
  -> AurelianEngine
  -> AurelianRuntimeSession
  -> AurelianFrameLoop
  -> runtime tick each frame
  -> frame pump
  -> Runtime compositor policy
  -> Core compositor bridge
  -> Vulkan compositor mechanism
  -> present
```

## Run

```bash
dotnet run --project samples/Aurelian.VisibleTriangle/Aurelian.VisibleTriangle.csproj -c Debug
```

Optional flags:

* `--validation` enables Vulkan validation when the plant is created.
* `--no-hold` presents through the finite frame loop and exits immediately instead of pumping the visible window for a short pause.
* `--frames 1` selects the finite frame count. A66 M0 supports one frame because the sample prepares one acquired swapchain image and presentation target; larger values are capped with a diagnostic.

## Expected behavior

When Vulkan presentation and a windowing environment are available, the sample opens a small window titled **Aurelian Visible Triangle**, renders a static triangle to an offscreen Vulkan color target, starts `AurelianEngine`, starts a Dominatus-backed `AurelianRuntimeSession`, runs `AurelianFrameLoop` for one finite frame, ticks runtime before the existing frame pump, dispatches the compositor passthrough path, presents through `IPresentationMechanism`, prints loop/runtime/frame diagnostics, stops runtime and engine, and exits.

If Vulkan, presentation, or the windowing platform is unavailable, the sample prints the typed diagnostics returned by the setup path and exits nonzero. This sample is intended for human/local runs; CI should build it but should not run it in headless environments.

## Boundaries

The sample deliberately does **not** implement an infinite game loop, editor host, `Aurelian.Host`, asset loading, world integration, render graph, input system, scheduler/threading system, differential compositor, runtime shader compilation, or a runtime DXC/SDSL dependency. The triangle shaders are tiny static SPIR-V byte fixtures copied into the sample so the executable can exercise the Vulkan compositor path without depending on test projects or shader tooling.

The sample still owns Vulkan/window/swapchain setup externally and passes prepared resources into Core. A67 should add safe per-frame swapchain acquire/present if the visible sample should run multiple frames.
