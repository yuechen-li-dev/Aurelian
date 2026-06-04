# Aurelian Dependency and Library Adoption Doctrine

## 1. Purpose

Aurelian should not reimplement every low-level or correctness-heavy subsystem from scratch. Physics solvers, navmesh builders, native graphics bindings, parsers, compression libraries, image codecs, and shader validation tools are all areas where subtle mistakes can consume large amounts of engineering time without improving the engine's architectural identity.

Aurelian should also not let external libraries define its architecture. A library may be the right way to solve bounded implementation plumbing, but it must not become the owner of the world model, runtime spine, actuator model, render architecture, asset contracts, shader language, or locality doctrine.

This policy defines when Aurelian should adopt libraries and how those dependencies must be kept behind clean Aurelian-owned boundaries before the engine begins adding physics, navigation, rendering, windowing, assets, or backend integrations.

## 2. Core rule

```text
Aurelian owns the architecture spine. Libraries provide bounded implementation plumbing.
```

The spine includes the world model, runtime, Dominatus orchestration, actuators, render snapshots/command plans, asset manifests, SDSL-V compiler artifacts, and locality doctrine. These are Aurelian-owned contracts and concepts.

Libraries may implement backend details behind those contracts. They can provide native API bindings, physics math and simulation internals, pathfinding queries, TOML syntax parsing, deterministic JSON support, image/codec infrastructure, or optional shader validation, but they must not replace Aurelian's architectural boundaries with their own object model.

## 3. When to use a library

| Good reason | Explanation |
| --- | --- |
| Correctness-heavy | Physics, navmesh, graphics bindings, parsers, compression, image codecs, etc. are easy to get subtly wrong. |
| Low strategic differentiation | Reimplementing it does not make Aurelian meaningfully better. |
| High implementation cost | Building it would delay the engine spine. |
| Clear boundary | Library can sit behind an Aurelian-owned interface/contract. |
| Replaceable later | The dependency can be swapped without rewriting the engine model. |
| NativeAOT-compatible or isolatable | Prefer libraries that work with NativeAOT; otherwise isolate them in non-core packages. |

## 4. When not to use a library

| Bad reason | Explanation |
| --- | --- |
| It is convenient but takes over architecture | Avoid importing another engine’s object model. |
| It requires service locator/global state patterns | Conflicts with locality and explicit contracts. |
| It forces reflection into core runtime | Conflicts with NativeAOT and locality. |
| It defines world/entity/component ownership | Aurelian owns its world model. |
| It couples frontend/editor/tooling to runtime core | Tools must remain layered. |
| It prevents deterministic tests | Core behavior should be testable headlessly. |

## 5. Specific library categories

### Silk.NET / Vortice

- Acceptable for native graphics/window/platform bindings.
- Must sit behind Aurelian rendering/HAL contracts.
- Must not define world, render snapshot, asset, or material architecture.
- Windowing should live outside core runtime.
- A22 introduces `Aurelian.Graphics` with `Silk.NET.Vulkan` and `Silk.NET.Windowing` references only as scaffold/package-smoke dependencies; A23 adds native-free plant contracts under `Aurelian.Graphics.Plants`; A24 adds optional/unavailable-safe Vulkan instance/device initialization for plant 0 only. A24 may create a Vulkan instance, physical-device selection, logical device, and selected queue when available, but it still creates no window, surface, swapchain, command buffers, resources, or renderer. A27 documents the allocator strategy before resource implementation: VMA/VMASharp may be used as backend plumbing, but Aurelian-owned allocator contracts and policy own the architecture seam. A28 implements those contracts plus a raw Vulkan allocator M0 backend; the raw backend is fallback plumbing only, and future buffers/textures must use `IVulkanMemoryAllocator` rather than direct Vulkan allocation calls.
- Vortice remains deferred until there is a concrete backend need that Silk.NET does not satisfy.
- Stride.Graphics remains reference-only and must not be ported into Aurelian-owned graphics code.

### BEPUphysics

- Acceptable for physics simulation.
- Aurelian should own physics component/store/actuator/event contracts.
- BEPU should not define entity identity or world ownership.
- Physics results should cross boundaries through typed events/snapshots.

### DotRecast

- Acceptable for navmesh/pathfinding.
- Aurelian should own navigation requests/results and world integration.
- DotRecast should not define AI behavior/policy.
- Dominatus/logic chooses goals; navigation library computes paths.

### Tomlyn

- Acceptable for TOML parsing.
- Tomlyn parses syntax; Aurelian owns asset manifest schema, validation, diagnostics, and artifact model.

### System.Text.Json

- Acceptable for deterministic JSON artifacts/manifests.
- Serialization shape should be explicit DTOs, not reflection-first runtime object graphs.

### DXC

- Acceptable as optional external shader validation/compiler tool.
- SDSL-V remains source language.
- DXC must not be required for normal tests.
- DXC output is artifact/compiler validation, not engine architecture.

### Avalonia / Machina

- Useful for tools/UI later.
- Must remain in tooling/editor packages.
- Must not become Aurelian core runtime dependency.

## 6. Reflection and NativeAOT policy

```text
Aurelian core is reflection-free by default.
```

Rules:

- no runtime reflection for core behavior;
- no assembly scanning for core systems;
- no `Activator.CreateInstance` dependency for core runtime;
- no reflective property paths;
- no reflection-defined serialization in runtime core;
- prefer source generation, manifests, typed registries, and explicit request/result contracts.

Reflection may be allowed:

- in tooling;
- in tests;
- in source generators;
- in build-time asset tools;
- only if the result becomes explicit generated/static metadata or artifacts.

```text
If the compiler cannot see the dependency, Aurelian should distrust it.
```

## 7. Dependency layering rules

- `Aurelian.Core` should stay minimal and dependency-light.
- `Aurelian.World` must not depend on rendering, assets, shaders, Dominatus, physics, navigation, or UI. World-owned renderable data is allowed only as symbolic world refs such as mesh/material strings, not rendering-contract refs or asset/shader handles.
- `Aurelian.Rendering.Contracts` contains renderer-independent DTOs only: render snapshots, items, cameras, resource refs, command plans, draw items, pass plans, symbolic pipeline/shader/target refs, statuses, reasons, diagnostics, and contract-local snapshot-to-plan assembly. It must not depend on world, assets, shaders, graphics/windowing packages, GPU handles, or backend object models.
- `Aurelian.Rendering.Null` is the first backend implementation boundary. It may depend on `Aurelian.Rendering.Contracts`, consumes command plans, and returns deterministic headless traces/results, but it must not introduce GPU/windowing packages, world, assets, shaders, backend-native handles, images, or windows.
- `Aurelian.Graphics` is the first graphics HAL boundary. It may reference `Aurelian.Rendering.Contracts` and tightly scoped graphics/windowing packages, starting with Silk.NET Vulkan/windowing, but it must not reference world, assets, shaders, the null renderer, Dominatus, `CodeReferences`, or vendor source. A23 adds PlantContext + PlantRegistry M0 as native-free plain data: `PlantId.Zero` is the one-plant M0 identity, the registry is deterministic and diagnostic-driven, presentation ownership is explicit, and no global graphics singleton is allowed. A24 adds a per-plant Vulkan instance/device owner: initialization is optional/unavailable-safe, created devices require timeline semaphores, the graphics+compute+transfer queue family is selected rather than hardcoded, and native handles do not leak into the plant model. A25 adds optional per-plant timeline semaphore fence wrappers and an explicit frame/command-list/copy fence bundle, plus a pure managed fence-tagged resource pool with telemetry for later command-buffer/descriptor/resource reuse; the pool is inspired by Stride and Prometheus patterns while avoiding literal engine-policy adoption. A26 adds a per-plant Vulkan command pool and primary command buffer lease lifecycle over that pool: command buffers reset/begin/end, retire with fence values, and reuse through the managed fence-tagged pool. A27 adds a docs-only Vulkan memory allocator strategy: Aurelian owns allocation contracts, budget facts, telemetry, fence retirement, grow/shrink policy, and future Dominatus hooks; VMA/VMASharp is a backend candidate that must stay hidden below `Aurelian.Graphics.Vulkan.Resources`. A26/A27 do not add buffers, textures, swapchains, windows, surfaces, render passes, draw/copy/update commands, or renderer execution. A28 adds allocator contracts and an isolated raw allocator backend. A29 adds the first real GPU resource type, Vulkan buffers, while preserving the allocator boundary: buffer creation may create/bind `VkBuffer` objects and request memory through `IVulkanMemoryAllocator`, but raw `vkAllocateMemory`/`vkFreeMemory` remain allocator-backend-only. A29 still does not add textures, mapped memory API, upload rings, staging/copy commands, descriptors, swapchains, windows, surfaces, render passes, pipelines, draws, VMA, Vortice, or dependencies on world/assets/shaders/null rendering/vendor/reference code.
- World-to-render extraction lives in `Aurelian.Runtime.Rendering` because runtime composition may reference both `Aurelian.World` and `Aurelian.Rendering.Contracts`; no separate extraction project is needed until the boundary becomes heavier.
- `Aurelian.Actuation` may depend on world contracts for world mutation.
- `Aurelian.Runtime` integrates Dominatus, dispatch, and world-to-render snapshot extraction. It may reference `Aurelian.Rendering.Contracts`, but production runtime must not reference `Aurelian.Rendering.Null`.
- Backend packages may depend on external libraries.
- Tools/editor packages may depend on UI/windowing libraries.
- No dependency should create reverse references into core layers.

## 8. Wrapper rule

For every external library, Aurelian should ask:

```text
Can this dependency be hidden behind an Aurelian-owned contract?
```

If no, do not adopt yet.

Examples:

- `IPhysicsWorld` / `PhysicsStepRequest` / `PhysicsStepResult` for BEPU.
- `INavigationMeshQuery` / `PathRequest` / `PathResult` for DotRecast.
- `IRenderBackend` / `RenderCommandPlan` for graphics.
- `AssetManifestParser` and Aurelian diagnostics for Tomlyn.
- `DxcValidator` for DXC.

## 9. Anti-goals

- no NIH rewrite of commodity infrastructure;
- no dependency capture;
- no reflection-first core;
- no editor/tool dependency in runtime;
- no adopting a full engine architecture just for rendering;
- no hidden global state from dependencies crossing into Aurelian’s core model.

## 10. First practical implications

- World typed data stores should stay library-free for now.
- Render snapshot and command-plan contracts are DTO-only in `Aurelian.Rendering.Contracts`.
- The null renderer is implemented in `Aurelian.Rendering.Null` as a headless backend over command plans. World-to-render extraction now exists in `Aurelian.Runtime.Rendering`, so `WorldDataDocument -> RenderSnapshot -> RenderCommandPlan -> NullRenderer` is testable headlessly.
- `Aurelian.Graphics` now owns the first graphics HAL scaffold, native-free plant registry, Vulkan instance/device initialization M0 for plant 0, optional per-plant timeline fence bundles, a pure managed fence-tagged resource pool, a per-plant Vulkan command pool / primary command buffer lease M0, Vulkan allocator contracts with an isolated raw backend, Vulkan Buffer resource M0 over `IVulkanMemoryAllocator`, A30 mapped CPU writes for host-visible buffers, and A31 one-shot staging-to-device-local buffer uploads through allocator/buffer/command/fence seams. `PlantId.Zero` represents the single-GPU plant in M0; successful Vulkan creation is per-plant, requires timeline semaphores, records plain facts/diagnostics, and keeps window/surface/swapchain/texture/rendering work deferred. VMA remains backend plumbing, not architecture.
- Visual render backends can later use Silk.NET first behind `Aurelian.Graphics`; Vortice remains deferred.
- Physics can later use BEPU behind `Aurelian.Physics.*`.
- Navigation can later use DotRecast behind `Aurelian.Navigation.*`.
- Assets can keep Tomlyn because TOML syntax is not strategic architecture.
- Shader DXC validation remains optional and external.

## A30 graphics mapped-memory note

A30 keeps CPU upload support inside existing dependency boundaries. The implementation uses Silk.NET Vulkan only at the backend edge and introduces no VMA/VMASharp, Vortice, global allocator, service locator, staging copy subsystem, textures, swapchain/window/surface, render pass, pipeline, or draw dependencies.

Mapped memory is represented through Aurelian-owned allocation and buffer contracts. Only allocator backends may call `vkMapMemory`/`vkUnmapMemory`; buffer code receives writability facts and performs bounds-checked writes through `AurelianVulkanBuffer.Write(...)` rather than owning raw memory allocation or mapping policy.


## A31 graphics upload dependency note

A31 keeps device-local buffer upload support inside the existing Aurelian-owned dependency boundaries. The upload helper may record `vkCmdCopyBuffer` and submit a command buffer, but it does not allocate, free, map, or unmap raw memory directly; staging memory is created through `VulkanBufferFactory` and `IVulkanMemoryAllocator`, and CPU writes go through `AurelianVulkanBuffer.Write(...)`. Command recording uses `VulkanCommandBufferPool`, and synchronization uses `VulkanFenceBundle.CommandListFence`.

The M0 upload path is intentionally synchronous: it waits for the submitted timeline fence value before disposing temporary staging resources. Upload rings, batching, persistent staging pools, async fence-retired staging, texture uploads, barriers/layout tracking, and renderer-facing draw infrastructure remain future work rather than new dependency policy.
