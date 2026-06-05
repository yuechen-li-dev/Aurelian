# Aurelian Visible Triangle sample

This A62 sample executable demonstrates the current prepared-visible Aurelian engine path:

```text
Core frame pump
  -> Runtime Dominatus compositor policy
  -> Core compositor bridge
  -> Graphics Vulkan compositor mechanism
  -> offscreen triangle
  -> compositor passthrough
  -> swapchain present
```

## Run

```bash
dotnet run --project samples/Aurelian.VisibleTriangle/Aurelian.VisibleTriangle.csproj -c Debug
```

Optional flags:

* `--validation` enables Vulkan validation when the plant is created.
* `--no-hold` presents once and exits immediately instead of pumping the visible window for a short pause.

## Expected behavior

When Vulkan presentation and a windowing environment are available, the sample opens a small window titled **Aurelian Visible Triangle**, renders a static triangle to an offscreen Vulkan color target, dispatches the compositor passthrough path through `AurelianFramePump.RunOneFrameAsync(...)`, presents the acquired swapchain image, prints status diagnostics, and exits.

If Vulkan, presentation, or the windowing platform is unavailable, the sample prints the typed diagnostics returned by the setup path and exits nonzero. This sample is intended for human/local runs; CI should build it but should not run it in headless environments.

## Boundaries

The sample deliberately does **not** implement a production frame loop, editor host, asset loading, world integration, render graph, differential compositor, runtime shader compilation, or a runtime DXC/SDSL dependency. The triangle shaders are tiny static SPIR-V byte fixtures copied into the sample so the executable can exercise the Vulkan compositor path without depending on test projects or shader tooling.
