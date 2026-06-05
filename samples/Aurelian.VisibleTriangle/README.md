# Aurelian Visible Triangle sample

This A67 sample executable demonstrates the current prepared-visible Aurelian engine spine across a small finite set of visible frames:

```text
sample-owned prepared Vulkan setup
  -> AurelianEngine
  -> AurelianRuntimeSession
  -> AurelianFrameLoop
  -> runtime tick each frame
  -> frame pump
  -> per-frame swapchain acquire
  -> Runtime compositor policy
  -> Core compositor bridge
  -> Vulkan compositor mechanism
  -> per-frame present
```

## Run

```bash
dotnet run --project samples/Aurelian.VisibleTriangle/Aurelian.VisibleTriangle.csproj -c Debug
```

Optional flags:

* `--validation` enables Vulkan validation when the plant is created.
* `--no-hold` presents through the finite frame loop and exits immediately instead of pumping the visible window for a short pause.
* `--frames N` selects a positive finite frame count. The default is `3`; values above `300` are capped to avoid an accidental long-running loop.

## Expected behavior

When Vulkan presentation and a windowing environment are available, the sample opens a small window titled **Aurelian Visible Triangle**, renders a static triangle once to an offscreen Vulkan color target, starts `AurelianEngine`, starts a Dominatus-backed `AurelianRuntimeSession`, and runs `AurelianFrameLoop` for the selected finite frame count.

For each frame, the sample-local input provider acquires a fresh swapchain image, creates a frame-specific `PresentationTargetRef`, creates frame-specific `AurelianFrameInput`, and records sample diagnostics. The runtime tick and compositor policy run each frame; the Core bridge dispatches the Vulkan compositor passthrough each frame; the sample-local presentation mechanism then presents the exact image index acquired for that completed frame.

The offscreen triangle is static/reused for M0. Setup creates finite `PlantOutputRef` wrappers for each planned frame ID, and each wrapper resolves to the same offscreen texture so this milestone exercises acquire/present lifecycle rather than animation or redraw scheduling.

If Vulkan, presentation, or the windowing platform is unavailable, the sample prints typed diagnostics returned by the setup or frame-loop path and exits nonzero. This sample is intended for human/local runs; CI should build it but should not run it in headless environments.

## Boundaries

The sample deliberately does **not** implement an infinite game loop, editor host, `Aurelian.Host`, asset loading, world integration, render graph, input system, scheduler/threading system, differential compositor, runtime shader compilation, or a runtime DXC/SDSL dependency. The triangle shaders are tiny static SPIR-V byte fixtures copied into the sample so the executable can exercise the Vulkan compositor path without depending on test projects or shader tooling.

The sample still owns Vulkan/window/swapchain setup externally and passes prepared resources into Core. Core frame loop and frame pump remain free of Vulkan/window/swapchain creation.
