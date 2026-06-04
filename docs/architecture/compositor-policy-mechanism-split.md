# Compositor policy/mechanism split

## 1. Purpose

A50 defines the compositor boundary before any compositor passthrough code is implemented. A49 proved presentation-aware plant creation, surface/swapchain creation, swapchain image views, binary presentation semaphores, typed `AcquireNextImage(...)`, and typed `Present(...)`; it deliberately did not render to the swapchain, add a compositor, or add a frame loop.

The next compositor work needs a clear contract seam because it sits between three very different concerns:

- neutral rendering contracts that describe frame facts, plant outputs, targets, requests, results, and diagnostics;
- runtime policy that can use Dominatus/HFSM/utility logic to decide what should happen this frame;
- graphics mechanism that owns Vulkan images, barriers, commands, submission, and presentation synchronization.

This document started as the A50 design audit. A51 has now implemented the neutral compositor contract layer in `Aurelian.Rendering.Contracts.Compositor`; the document still does not authorize swapchain image wrapping, image copy/blit, Vulkan commands, Dominatus runtime policy, or a frame loop.

## 2. What the compositor is and is not

The compositor is the graphics mechanism that converts one or more plant output images into the image that will be presented. In M0, the first useful mechanism should be passthrough: one ready plant output is copied/blitted/rendered into the acquired presentation target and then presented through the A49 presentation seam.

The compositor is not the entire runtime decision system. The decision of whether to composite, which inputs are required, whether to use passthrough/full-quality/reduced-frequency/differential behavior, and whether confidence or cadence permits a cheaper path is policy. That policy belongs in runtime/Dominatus, not in Vulkan object owners.

The compositor is also not a world-model feature. It should not query world units directly, own plant scheduling, hide frame-loop policy, or become a renderer facade that reintroduces global graphics state.

## 3. Policy vs mechanism

```text
Compositor policy (runtime/Dominatus):
  observes neutral frame facts;
  decides which plant outputs are required for the selected policy;
  chooses Passthrough, FullQuality, ReducedFrequency, or Differential;
  handles confidence, agreement, cadence, and wait decisions;
  emits neutral compositor dispatch requests/acts;
  stores policy state in Dominatus blackboard/HFSM/utility structures.

Compositor mechanism (graphics/Vulkan):
  resolves neutral plant-output and presentation-target references to Vulkan images;
  owns swapchain image wrappers, plant output image wrappers, barriers, copy/blit/compute pipelines, command recording, submission, and semaphore handoff;
  executes a neutral compositor dispatch request;
  returns neutral dispatch results and diagnostics;
  never depends on Dominatus or runtime policy types.
```

The two layers communicate through typed contracts. Contracts must be DTOs only: no Vulkan handles, no Silk.NET structs, no Dominatus types, no `Aurelian.Graphics` object references, and no world objects.

## 4. Contract layer

Neutral compositor contracts should live under:

```text
src/Aurelian.Rendering.Contracts/Compositor/
```

This matches existing renderer-independent render snapshots, command plans, and compiled shader contracts. Both `Aurelian.Runtime` and `Aurelian.Graphics` already depend on `Aurelian.Rendering.Contracts`, so this location allows policy and mechanism to share DTOs without either layer depending on the other.

A51 implemented the M0 contracts as neutral DTOs:

```csharp
public enum CompositorPolicyKind
{
    Passthrough,
    FullQuality,
    ReducedFrequency,
    Differential,
}

public readonly record struct PlantOutputRef(
    uint PlantId,
    ulong FrameId,
    string ImageId);

public readonly record struct PresentationTargetRef(
    uint PlantId,
    uint SwapchainImageIndex,
    ulong FrameId);

public enum PlantOutputReadinessStatus
{
    Missing,
    Pending,
    Ready,
    Reused,
    Failed,
}

public sealed record PlantOutputReadiness(
    PlantOutputRef Output,
    PlantOutputReadinessStatus Status,
    ulong? CompletedFenceValue = null,
    string? DiagnosticCode = null);

public sealed record RequiredPlantOutputSet(
    ulong FrameId,
    CompositorPolicyKind Policy,
    IReadOnlyList<PlantOutputRef> RequiredOutputs);

public sealed record CompositorDiagnostics(
    double? AgreementRate,
    IReadOnlyDictionary<string, double> Metrics);

public sealed record CompositorFrameFacts(
    ulong FrameId,
    IReadOnlyList<PlantOutputReadiness> Outputs,
    CompositorDiagnostics PreviousDiagnostics,
    double? ShadowCalibrationConfidence);

public sealed record CompositorDispatchRequest(
    ulong FrameId,
    CompositorPolicyKind Policy,
    IReadOnlyList<PlantOutputRef> Inputs,
    PresentationTargetRef Target);

public enum CompositorDispatchStatus
{
    Dispatched,
    Skipped,
    Rejected,
    Failed,
}

public sealed record CompositorDispatchDiagnostic(
    string Code,
    CompositorDispatchDiagnosticSeverity Severity,
    string Message);

public sealed record CompositorDispatchResult(
    CompositorDispatchStatus Status,
    ulong FrameId,
    CompositorPolicyKind Policy,
    PresentationTargetRef Target,
    CompositorDiagnostics Diagnostics,
    IReadOnlyList<CompositorDispatchDiagnostic> DispatchDiagnostics);
```

M0 keeps this set intentionally small. It names images and targets symbolically rather than exposing backend handles. `RequiredPlantOutputSet.IsSatisfiedBy(...)` is pure readiness matching for policy tests: every required output must have matching `Ready` or `Reused` readiness, and extra readiness entries are ignored. If a later milestone needs richer identities, add fields to these contracts instead of leaking Vulkan owners into runtime policy.

## 5. Runtime Dominatus policy proposal

Runtime policy should live under:

```text
src/Aurelian.Runtime/Compositor/
```

`Aurelian.Runtime` is the current composition layer: it already references world data, rendering contracts, and Dominatus. That makes it the right place to translate frame facts into policy decisions while keeping graphics backend execution separate.

Potential future runtime types:

- `CompositorPolicySession`: owns the Dominatus `AiWorld`, policy agent, blackboard keys, and tick/update method for compositor policy only.
- `CompositorPolicyFacts`: runtime-facing wrapper or adapter around `CompositorFrameFacts` if session-local bookkeeping is needed.
- `CompositorPolicyResult`: reports wait/dispatch decisions without executing graphics commands.
- `CompositorDispatchAct`: a Dominatus actuation command whose payload is a `CompositorDispatchRequest`.
- `CompositorPolicyGraphBuilder`: builds the HFSM root/options after the neutral mechanism contract exists.

The policy session should observe `CompositorFrameFacts`, decide whether all required outputs for the selected policy are ready, and either emit a compositor dispatch act or wait. It should not contain Vulkan image handles, command buffers, semaphores, swapchain owners, or plant resource wrappers.

## 6. Graphics Vulkan mechanism proposal

The Vulkan mechanism should live under:

```text
src/Aurelian.Graphics/Vulkan/Compositor/
```

The repository already contains this folder as an empty graphics backend area, and it is a cleaner top-level mechanism home than burying the compositor below presentation. Presentation owns surface/swapchain acquire/present; the compositor will need presentation targets, but it will also need plant output images, command buffers, barriers, pipelines, resource lookup, and submit integration. A top-level Vulkan compositor folder keeps those concerns near Vulkan mechanism code without implying that presentation policy owns composition.

Potential future graphics types:

- `AurelianVulkanCompositor`: executes `CompositorDispatchRequest` values and returns `CompositorDispatchResult` values.
- `VulkanCompositorPassthrough`: first M0 mechanism variant after contracts exist.
- `VulkanCompositorDispatchActuator`: optional adapter that turns a Dominatus actuation command into a call to the graphics compositor, while the handler itself remains in runtime/host composition rather than forcing Dominatus into graphics.
- `VulkanPlantOutputImage`: backend wrapper that maps a `PlantOutputRef` to an actual Vulkan image/image view/layout tracker.
- `VulkanPresentationTargetImage`: backend wrapper that maps `PresentationTargetRef` and acquired swapchain image index to a Vulkan swapchain image/image view wrapper.

The mechanism should use existing seams where possible: presentation acquire/present results, command-buffer leases, barrier command emission, and queue submit helpers. It should return diagnostics rather than hiding out-of-date, suboptimal, unavailable, or synchronization failures behind exceptions.

## 7. Claude HFSM sketch assessment

Accepted:

- The policy layer can be Dominatus HFSM/blackboard/utility logic.
- No new control primitive is required for compositor policy M0; Dominatus already has blackboard keys, HFSM nodes, utility decisions, acts, waits, and actuator dispatch.
- Compositor execution is naturally an actuator boundary: policy emits an act/request; mechanism completes it with a result.
- Differential-rendering confidence is policy data: agreement rate, calibration confidence, and scene-type metrics can be blackboard facts and utility/HFSM inputs.

Refined:

- “All plants done” should become “all required outputs for the selected policy are ready.” Passthrough may require one output; full-quality may require more; reduced-frequency may intentionally reuse an older trusted output.
- Policy, mechanism, diagnostics, and contracts are separate layers even if a future runtime session ticks them in one frame loop.
- Readiness, quality/trust, cadence, and dispatch are separate concerns. They can be coordinated by one Dominatus policy graph, but their data should stay explicit.
- Vulkan resources must not be stored in Dominatus state.
- Dominatus must not be referenced by `Aurelian.Graphics`.

## 8. First Dominatus use recommendation

Use Dominatus first for runtime compositor policy, but only after a neutral contract package exists and the first graphics passthrough mechanism is at least contract-shaped. The practical sequence is:

1. define compositor DTOs in `Aurelian.Rendering.Contracts`;
2. build the graphics passthrough mechanism against those DTOs;
3. then introduce a small Dominatus policy session that observes `CompositorFrameFacts` and emits `CompositorDispatchRequest` when required outputs are ready.

This avoids designing Dominatus graph state around nonexistent mechanism details, and it prevents Vulkan code from absorbing policy decisions just to make passthrough work.

## 9. Milestone plan A51+

Recommended sequence:

```text
A51 — Compositor contracts M0
  Add neutral DTOs under Aurelian.Rendering.Contracts/Compositor.
  No Vulkan, no Dominatus, no runtime policy, no graphics implementation.

A52 — Swapchain image wrappers M0
  Wrap acquired swapchain images/image views as backend-owned presentation target images.
  No copy/blit compositor yet.

A53 — Vulkan compositor passthrough copy M0
  Execute a CompositorDispatchRequest by copying/blitting one ready plant output into the acquired presentation target.
  Use barriers, command recording, submit, and render-finished semaphore handoff explicitly.

A54 — Runtime Dominatus compositor policy M0
  Add the first runtime policy session using Dominatus blackboard/HFSM/acts to decide dispatch-or-wait for passthrough/full-quality M0 facts.

A55 — First visible triangle through compositor path
  Connect offscreen draw output through compositor passthrough to presentation in a minimal frame loop proof.
```

A51 implemented neutral contracts first. A52 then added graphics-side swapchain image wrappers after the policy/mechanism seam was explicit.

## 10. Anti-goals

- No Vulkan handles in contracts.
- No Silk.NET structs in contracts.
- No Dominatus dependency in `Aurelian.Graphics`.
- No `Aurelian.Graphics` dependency in runtime policy contracts.
- No policy hidden in the Vulkan compositor.
- No compositor direct world dependency.
- No multi-GPU implementation in M0.
- No differential rendering until passthrough works.
- No frame loop or renderer facade as part of the contract milestone.
- No vendor/Dominatus or CodeReferences modifications for compositor M0.


## 11. A51 implementation status

A51 implements the neutral compositor DTO layer in `Aurelian.Rendering.Contracts.Compositor`. It includes policy kinds, symbolic plant output and presentation target references, readiness facts, required-output satisfaction, diagnostics, frame facts, dispatch requests, dispatch statuses, dispatch diagnostics, and dispatch results.

A51 intentionally adds no Vulkan/Silk handles, no graphics mechanism, no Dominatus/runtime policy, no world dependency, no new packages, and no new project references. The next recommended milestone is **A52 — Swapchain image wrappers M0**, which should make acquired presentation images addressable by the graphics mechanism while keeping contracts neutral.


## 12. A52 implementation status

A52 implements graphics-side swapchain image wrappers under `Aurelian.Graphics.Vulkan.Compositor`. The wrappers are backend mechanism targets, not neutral contracts and not ordinary allocated textures. `AurelianVulkanSwapchain.CreatePresentationTargetImageSet()` creates one non-owning wrapper per swapchain image, preserving image order and carrying plant ID, swapchain image index, format, extent, internal native image/image-view handles, and a per-image one-mip/one-layer layout tracker initialized to `Present`.

`VulkanPresentationTargetResolver` maps neutral `PresentationTargetRef` values to backend wrappers and rejects missing image sets, plant mismatches, and out-of-range image indices with typed diagnostics. A52 still emits no barriers, copy/blit commands, render commands, queue submits, acquire calls, present calls, frame loop code, or Dominatus policy. The next recommended milestone is **A53 — Vulkan compositor passthrough copy M0**.
