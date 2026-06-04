# Aurelian MVP Roadmap

- **A0 — Bootstrap:** create the clean solution, strict build discipline, architecture charter, project skeleton, and smoke tests without external runtime/vendor links. **Completed.**
- **A1 — Vendor Dominatus runtime smoke:** vendor Dominatus under `vendor/Dominatus/`, add buildable Dominatus projects to the solution, and add the first runtime smoke while keeping renderer work out of scope. **Completed.**
- **A2 — SDSL-V convergence audit:** establish WyrmCoil Rust SDSL-V as reference/inspiration, classify `src/StriV.ShaderPipeline` as migration scaffold, and keep Stride SDSL/mixins as historical reference rather than native Aurelian input. **Completed.**
- **A3 — Aurelian.Shaders identity conversion:** convert the carried-over `StriV.ShaderPipeline` identity into the linked `Aurelian.Shaders` module and add minimal smoke tests while preserving current behavior. **Completed.**
- **A4 — Aurelian.Shaders AST convergence:** added the first WyrmCoil-shaped Aurelian SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`, with tests for paths, type refs, modules, records, streams, enums, shader declarations, expressions, and flow declarations. Legacy parser/lowerer behavior remains untouched. **Completed.**
- **A5 — SDSL-V parser convergence M0:** added the first token-driven SDSL-V parser path under `Aurelian.Shaders.Language`, covering namespace/use/type refs/records/streams/enums/shader shells plus small function/body expression support. Legacy parser/lowerer behavior remains untouched and HLSL emission is unchanged. **Completed.**
- **A6 — SDSL-V parser M1 statements/expressions:** expanded the new parser path with precedence, postfix chains, array literals, with/switch/match/fallibility parse shapes, if/else, for, assignment, expression, empty, let, and return statements; added the WyrmCoil ↔ Aurelian SDSL-V compatibility matrix. **Completed.**
- **A7 — SDSL-V validation M0:** added structural validation over the new AST with xUnit coverage for duplicate module/member/local names, basic type-reference validity, and positive array lengths. Full type checking, `.sdslvtest`, Oct interpreter CPU simulation, and HLSL emission remain deferred. **Completed.**
- **A8 — HLSL emission M0 over new AST:** added deterministic HLSL emission over the new parser/AST/validator path, a checked-in smoke `.sdslv` fixture, and xUnit coverage for record structs, stage functions, and unsupported-construct diagnostics. No DXC/SPIR-V, renderer, `.sdslvtest`, or CPU shader evaluation is included. **Completed.**
- **A9 — SDSL-V artifact manifest M0:** package SDSL-V source identity, SHA-256 source hash, emitted HLSL, diagnostics, success state, and heuristic stage/profile entry point metadata into deterministic in-memory artifacts and JSON manifests. No DXC/SPIR-V, renderer, asset/TOML bridge, `.sdslvtest`, or CPU shader evaluation is included. **Completed.**
- **A10 — Optional DXC validation M0:** add an optional `Aurelian.Shaders.Language.External.Dxc` validation layer for generated HLSL artifacts. It validates HLSL syntax/profile/entry point combinations when DXC is available through `AURELIAN_DXC`, `dxc`, or `dxc.exe`, and normal builds/tests skip cleanly when DXC is missing. No SPIR-V, Vulkan SDK, renderer, or asset/TOML bridge is included. **Completed.**
- **A11 — Aurelian asset identity conversion:** convert the remaining carried-over `StriV.AssetPipeline` and `StriV.AssetTool` projects into linked `Aurelian.Assets` and `Aurelian.AssetTool` modules with smoke tests and no broad asset-system redesign. **Completed.**
- **A12 — World model doctrine:** define the Aurelian world/scene/component/actuator design doctrine before implementing `Aurelian.World`, with locality of change as the primary design law, data/composition/logic as the uniform model, `WorldUnit` as the conceptual locality boundary, and an explicit rejection of ECS processor architecture. **Completed.**
- **A13 — World Unit M0:** implement the smallest possible world model: `UnitId`, `UnitKindId`, `WorldUnitDescriptor`, `UnitChild`, `UnitComposition`, opaque `UnitLogicRef`, `WorldDocument`, `ResolvedWorld`, a simple parent/child resolver, deterministic unit/child snapshot queries, diagnostics, and tests for locality boundaries and shallow composition. No behavior runtime, Dominatus integration, actuators, renderer, assets, physics, LLM calls, blackboards, full ECS, processors, entity manager, or deep inheritance system is included. **Completed.**
- **A14 — Actuator request/result M0 over WorldUnit:** define the first typed mutation boundary over world unit documents with request/result contracts and tests, while keeping behavior runtime, Dominatus orchestration, renderer, assets, physics, and blackboards out of scope. **Completed.**
- **A15 — Dependency policy:** document the Aurelian dependency and library adoption doctrine before physics, navigation, rendering, windowing, asset, or backend integrations begin. Useful libraries may be used behind Aurelian-owned contracts, while core layers remain explicit, NativeAOT-oriented, and reflection-free by default. **Completed.**
- **A16 — World typed data stores M0:** add minimal typed stores for world data while keeping mutation through actuators, keeping `Aurelian.World` library-free, and avoiding behavior/runtime/render/assets integration. Current stores are unit name/label and 2D transform only, exposed through `WorldDataDocument` and `WorldDataSnapshot`; they are explicit typed stores rather than ECS components. **Completed.**
- **A17 — Render snapshot contracts M0:** add renderer-independent DTO contracts in `Aurelian.Rendering.Contracts.Snapshots` for frame snapshots, 2D cameras, 2D render items, typed mesh/material/texture refs, statuses, and diagnostics. No extraction project, world reference, renderer/backend, asset/shader dependency, graphics/windowing package, or new production project split is included. **Completed.**
- **A18 — Render command plan contracts M0:** add backend-independent command-plan DTO contracts in `Aurelian.Rendering.Contracts.CommandPlans` for symbolic pipeline/shader/target refs, 2D draw items, pass plans, command-plan statuses/reasons/diagnostics, and a tiny snapshot-to-plan builder. No new production project, world extraction, null renderer/backend, asset/shader dependency, graphics/windowing package, GPU handle, or backend-native concept is included. **Completed.**
- **A19 — Null renderer M0:** add `Aurelian.Rendering.Null` as the first backend-shaped implementation over `RenderCommandPlan`. It consumes ready/empty/rejected plans, validates malformed ready plans, and returns deterministic pass/draw traces and diagnostics without GPU/windowing, assets, shaders, world extraction, or real rendering. The new project is justified by a real backend dependency boundary while `Aurelian.Rendering.Contracts` stays pure DTO/contracts. **Completed.**
- **A20 — World-to-render snapshot extraction M0:** connect world data stores to renderer-independent snapshots without adding backend dependencies to world. **Completed.**
- **A21 — First graphics backend decision audit:** choose the first visual backend direction and document the Vulkan intent-port plan. Silk.NET Vulkan/windowing is the first backend path, Vortice is deferred, Stride.Graphics remains reference-only, and the plant/controller model is preserved. **Completed.**
- **A22 — Aurelian.Graphics scaffold:** create `Aurelian.Graphics` and `tests/Aurelian.Graphics.Tests`, add Silk.NET Vulkan/windowing package references, and prove package visibility without creating a Vulkan instance, window, surface, device, swapchain, command buffers, renderer, resources, or plant registry. **Completed.**
- **A23 — PlantContext + PlantRegistry M0:** define `PlantId`, `PlantContext`, `PlantRegistry`, plant selection, and graphics diagnostics DTOs for one fixed native-free plant descriptor/context. `PlantId.Zero` is the single-GPU M0 plant, registry order is deterministic, there is no global graphics singleton, and Vulkan instance/device/window/surface creation remains A24. **Completed.**
- **A24 — Vulkan instance/device initialization M0:** create per-plant Vulkan instance/device ownership for plant 0 when the loader/runtime and required capabilities are available, while returning unavailable diagnostics on machines without Vulkan and still avoiding window, surface, swapchain, command buffers, buffers, textures, resources, and renderer work. **Completed.**
- **A24b — Dominatus vendor expansion:** vendor and solution-link `Ariadne.OptFlow` and `Dominatus.UtilityLite` under `vendor/Dominatus/src/`, and copy `Ariadne.Console`, `Dominatus.Fishtank`, `Dominatus.TinyTown`, and `Dominatus.RTSBenchmark` under `vendor/Dominatus/samples/` as reference-only Codex authoring material. Samples are not linked in `Aurelian.slnx` and Aurelian production projects do not reference them. **Completed.**
- **A25 — Timeline fences and resource pool M0:** add optional per-plant Vulkan timeline semaphore fence wrappers, an explicit frame/command-list/copy fence bundle, and a pure managed fence-tagged FIFO resource pool with telemetry. Tests skip cleanly when Vulkan is unavailable, and the milestone still avoids windows, surfaces, swapchains, command buffers, buffers, textures, rendering resources, VMA, and Vortice. **Completed.**
- **A26 — Vulkan command buffer pool M0:** add a per-plant Vulkan command pool, primary command buffer leases with reset/begin/end lifecycle, fence-tagged retirement/reuse through the A25 pool foundation, and command-buffer telemetry. Tests skip cleanly when Vulkan is unavailable, and the milestone still avoids windows, surfaces, swapchains, render passes, draw/copy/update commands, buffers, textures, VMA, Vortice, and renderer/backend execution. **Completed.**
- **A27 — Vulkan memory allocator strategy audit:** document the allocator strategy before buffers/textures. VMA/VMASharp is evaluated as backend plumbing, not architecture; Aurelian owns allocation contracts, budget facts, telemetry, retirement, grow/shrink policy, and future Dominatus hooks. No packages, code, project changes, buffers, or textures are added. **Completed.**

## Shader pipeline status

A3 converted `src/StriV.ShaderPipeline/` to `src/Aurelian.Shaders/` and linked `Aurelian.Shaders` as an Aurelian module in `Aurelian.slnx`. A4 added a separate WyrmCoil-shaped SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`; the carried-over legacy AST/parser/lowerer and artifact emitter remain temporarily in place with preserved behavior.

Aurelian SDSL-V semantics are now represented by the new AST contract, and A5 added the first token-driven parser path under `Aurelian.Shaders.Language.Tokens`, `Aurelian.Shaders.Language.Lexing`, `Aurelian.Shaders.Language.Diagnostics`, and `Aurelian.Shaders.Language.Parsing`. A6 expanded the parser to M1 statements/expressions and added `docs/architecture/sdslv-compatibility-matrix.md` to track WyrmCoil reference compatibility, intentional Aurelian divergences, and deferred features. A7 added `Aurelian.Shaders.Language.Validation` for structural validation M0 and stable validation diagnostic codes. A8 added `Aurelian.Shaders.Language.Emission.Hlsl` for HLSL emission M0 over the new AST, plus `tests/Aurelian.Shaders.Tests/Fixtures/Sdslv/smoke_triangle.sdslv` as a readable source fixture that parser/validator/emitter tests consume. A9 added `Aurelian.Shaders.Language.Artifacts` for artifact manifest M0 over `source -> parse -> validate -> HLSL emit -> artifact/manifest`; the artifact includes source identity, SHA-256 hash, emitted HLSL, combined diagnostics, success state, and heuristic stage/profile entry points, and the M0 JSON manifest includes HLSL directly until a later asset pipeline can split files. A10 added `Aurelian.Shaders.Language.External.Dxc` for optional external DXC validation of generated HLSL; `DxcDiscovery` checks `AURELIAN_DXC`, then `dxc`, then `dxc.exe` on `PATH`, and `DxcValidator` returns skipped statuses when DXC, HLSL, or stage entry points are unavailable. WyrmCoil remains reference-only: semantics are compared and copied conceptually where useful, not referenced as code. Aurelian SDSL-V is its own production C# implementation, with no native old Stride mixins or old Stride effect/base-shader inheritance model. `.sdslvtest` is deferred; a future design may mirror `.octest` by porting the Oct interpreter architecture to C# and using deterministic CPU shader behavior simulation after parser, validation, emission, and artifact contracts are stable. SPIR-V/Vulkan output and renderer/windowing remain deferred; DXC is optional HLSL validation only and normal build/test commands do not require it. A11 converted the remaining carried-over Stri-V asset projects into `Aurelian.Assets` and `Aurelian.AssetTool`. A12 added `docs/architecture/world-model-doctrine.md`, establishing locality of change as the primary design law, data/composition/logic as the uniform world model, components as reusable local composition units, `WorldUnit` as the conceptual locality boundary, and actuators/Dominatus/render/assets as separate responsibilities. A13 implemented World Unit M0 with descriptors, immediate child composition, opaque logic references, a resolver, diagnostics, and deterministic resolved-world queries. A14 added `Aurelian.Actuation.World` request/result contracts and `WorldUnitActuator` for deterministic immutable-style `WorldDocument` mutations; applied changes return a new document validated by the resolver, and rejected/no-op outcomes keep the original document with structured diagnostics. A15 added `docs/architecture/dependency-policy.md`, establishing that useful libraries may be adopted for correctness-heavy, low-ROI plumbing only behind Aurelian-owned contracts; core runtime remains explicit, NativeAOT-oriented, and reflection-free by default. A16 implemented World typed data stores M0 so unit composition can carry richer local data without collapsing into global processors, runtime blackboards, or dependency-defined object models. Store values are unit names/labels and simple 2D transforms only; mutation flows through typed `Aurelian.Actuation.World` requests/results, and `Aurelian.World` remains library-free. Future stores may include 3D transform, renderable references, camera references, physics state, or navigation state behind Aurelian-owned contracts. A17 Render snapshot contracts M0 and A18 Render command plan contracts M0 provide backend-independent DTO boundaries without adding renderer/assets/shader dependencies to world. A19 Null renderer M0 adds the first backend implementation as a headless consumer of command plans, returning deterministic trace/result records with no GPU/windowing packages. A20 World-to-render snapshot extraction M0 adds symbolic world renderable data and `Aurelian.Runtime.Rendering` extraction so `WorldDataDocument -> RenderSnapshot -> RenderCommandPlan -> NullRenderer` is testable headlessly without GPU/window/assets/shader integration. A21 through A26 establish the first Vulkan direction, graphics scaffold, plant registry, instance/device initialization boundary, optional timeline fence bundle, pure managed fence-tagged resource pool, and per-plant command pool / primary command buffer lifecycle. A27 adds the Vulkan memory allocator strategy audit before buffer/texture implementation: VMA/VMASharp is backend plumbing, not architecture, and Aurelian-owned allocator contracts/policy own the seam. A24b expands the Dominatus vendor/reference set without changing Aurelian production dependencies: `Ariadne.OptFlow` and `Dominatus.UtilityLite` are linked build modules, while `Ariadne.Console`, `Dominatus.Fishtank`, `Dominatus.TinyTown`, and `Dominatus.RTSBenchmark` remain reference-only samples for Codex authors. The next implementation step should be A28 Vulkan allocator contracts + raw allocator M0 so buffer ownership and retirement can be layered over explicit allocator contracts, the A25/A26 fence/pool foundations, and a replaceable VMA backend seam.

`src/StriV.AssetPipeline` and `src/StriV.AssetTool` have been consumed by `src/Aurelian.Assets` and `src/Aurelian.AssetTool`. `Aurelian.Assets` owns early TOML/manifest orchestration and may need schema convergence; it must not use the Stride asset system. `CodeReferences/*` remains reference-only and must not be linked, compiled, or modified as part of Aurelian module work.

## A20 — World-to-render snapshot extraction M0

A20 connects the existing typed world data stores to renderer-independent render snapshots without adding a visual backend or a new extraction project.

Implemented path:

```text
WorldDataDocument -> RenderSnapshot -> RenderCommandPlan -> NullRenderer
```

Key decisions:

- `Aurelian.World` owns `Renderable2DData` and `Renderable2DStore` as explicit typed world data stores.
- World renderable refs are symbolic `WorldMeshRef` and `WorldMaterialRef` string values only.
- `Aurelian.World` still has no dependency on `Aurelian.Rendering.Contracts`, assets, shaders, GPU APIs, or windowing.
- Extraction glue lives in `Aurelian.Runtime.Rendering`, where references to both world data and rendering contracts are allowed.
- `Aurelian.Runtime` does not reference `Aurelian.Rendering.Null` in production; the null renderer is used only by tests to prove the headless chain.

Deferred beyond A20:

- visual/GPU backend selection;
- shader asset manifest integration;
- asset/material loading;
- camera stores beyond the default symbolic M0 camera;
- Dominatus world observation bridge.

## A22 — Aurelian.Graphics scaffold

A22 creates `Aurelian.Graphics` as the first graphics HAL project and `tests/Aurelian.Graphics.Tests` as its package-smoke test boundary. The project references `Aurelian.Rendering.Contracts` plus `Silk.NET.Vulkan` and `Silk.NET.Windowing` only; `Silk.NET.Core` remains transitive and is not centrally pinned.

A22 deliberately does not create a Vulkan instance, window, surface, device, swapchain, command buffer, renderer, plant registry, or resources. Vortice and VMA remain deferred, and Stride.Graphics remains a reference-only pitfall corpus. A27 later clarifies that VMA/VMASharp should be considered only as allocator backend plumbing behind Aurelian-owned allocation contracts and policy.

A23 adds native-free PlantContext + PlantRegistry M0: `PlantId.Zero`, one fixed Vulkan plant context, deterministic registry diagnostics, explicit presentation ownership, no native handles, and no global graphics singleton.

A24 adds Vulkan instance/device initialization M0 for plant 0. The initializer creates a per-call Vulkan API object, instance, selected physical device, logical device, and selected queue only when the Vulkan loader/runtime and required device capabilities are available. Normal tests remain green on machines without Vulkan by returning unavailable/rejected statuses with diagnostics. Created devices require timeline semaphore support, the queue family must support graphics + compute + transfer and is selected rather than hardcoded, and A24 still creates no window, surface, swapchain, command buffers, resources, or renderer.

A24b adds the Dominatus vendor-maintenance expansion before Vulkan work continues: `Ariadne.OptFlow` and `Dominatus.UtilityLite` are build-linked under `vendor/Dominatus/src/`, while `Ariadne.Console`, `Dominatus.Fishtank`, `Dominatus.TinyTown`, and `Dominatus.RTSBenchmark` are copied under `vendor/Dominatus/samples/` as reference-only Codex authoring material and are not linked into `Aurelian.slnx`.

A25 adds per-plant timeline semaphore wrappers and native-free pool contracts while still avoiding window/surface/swapchain, command buffers, buffers, textures, and rendering resources. Fence bundles are explicit (`VulkanFenceBundle.Create(plant)`) rather than automatically created by device initialization, keeping A24 plant creation stable and allowing tests to skip cleanly when Vulkan is unavailable. The pure managed fence-tagged pool is FIFO and telemetry-driven so later Vulkan resource layers can reuse objects only after the relevant fence has completed.

A26 adds the first native command-buffer lifecycle layer over A25: one command pool per plant, selected queue family use instead of hardcoded family 0, primary command buffer leases with reset/begin/end, fence-tagged retire/reuse, and command-buffer pool telemetry. A26 records no draw, copy, update, render-pass, buffer, texture, swapchain, surface, window, or backend execution work.

A28 adds Vulkan allocator contracts + raw allocator M0: Aurelian-owned allocation requests/results, `GpuResourceState`, allocation handles, status/diagnostic records, per-plant allocator telemetry, `IVulkanMemoryAllocator`, and an isolated raw Vulkan allocator backend. The raw backend is an M0 fallback only and centralizes memory type selection plus `vkAllocateMemory`/`vkFreeMemory`; future buffers/textures must depend on `IVulkanMemoryAllocator`. VMA/VMASharp remains deferred behind the same boundary once package/API/NativeAOT behavior is verified.

A29 adds Vulkan Buffer resource M0: Aurelian-owned usage flags and create plans, `VkBuffer` creation, memory requirement queries, `IVulkanMemoryAllocator` allocation requests, `vkBindBufferMemory`, plant-owned `GpuResourceState`, safe idempotent disposal, and unavailable-safe tests. Buffer code does not call `vkAllocateMemory`/`vkFreeMemory`; those remain isolated to allocator backends. A29 still adds no textures, upload rings, mapped memory API, staging/copy commands, descriptors, swapchains, windows, surfaces, render passes, pipelines, or draw work.

The next implementation step should be A30 Buffer mapped memory / upload M0 so CPU-visible and staging upload paths are explicit before vertex-buffer or texture milestones consume real data.

## A30 — Buffer mapped memory / CPU upload M0

A30 adds safe host-visible buffer writes to `Aurelian.Graphics`. Allocation requests can opt into persistent mapping with `MapOnCreate` for `CpuToGpu`/`GpuToCpu` memory, `GpuOnly` mapping is rejected with diagnostics, and raw Vulkan map/unmap calls remain isolated to `RawVulkanMemoryAllocator`. `AurelianVulkanBuffer.Write(ReadOnlySpan<byte>, ulong)` is the M0 CPU upload API for mapped buffers and performs disposed, mapped/writable, and bounds checks before copying bytes.

A30 deliberately does not add staging-to-device-local copies, command buffer upload submission, an upload ring, non-coherent flush/invalidate support, textures, descriptor binding, render passes, pipelines, drawing, swapchains/windows/surfaces, VMA/VMASharp, or Vortice. The recommended next milestone is `A31 — Staging buffer / device-local upload copy M0`.


## A31 — Staging buffer / device-local upload copy M0

A31 adds the first real CPU-to-GPU device-local buffer upload path in `Aurelian.Graphics`. The one-shot `VulkanBufferUploader` validates a destination buffer, creates a mapped `CpuToGpu` transfer-source staging buffer through `VulkanBufferFactory` and `IVulkanMemoryAllocator`, writes bytes through `AurelianVulkanBuffer.Write(...)`, records `vkCmdCopyBuffer` into a lease from `VulkanCommandBufferPool`, submits the command buffer to the plant queue, signals `VulkanFenceBundle.CommandListFence`, waits synchronously for that timeline value, retires the command buffer, and disposes staging resources only after completion.

A31 deliberately keeps M0 narrow: no upload ring, persistent staging allocator, batching, async fence-retired staging resources, texture upload, descriptor binding, swapchain/window/surface, render pass, pipeline, draw path, VMA/VMASharp, Vortice, or cross-module renderer dependency is added. The recommended next milestone is `A32 — Barrier/layout tracker M0` because later texture/resource work will need explicit transition infrastructure.

## A32 — Barrier/layout tracker M0 note

A32 implements the pure barrier-planning foundation that must exist before texture resources, render passes, and pipeline/draw work. `Aurelian.Graphics` now owns a Vulkan resource layout vocabulary (`Undefined`, transfer layouts, stage-specific shader-resource layouts, storage read/write, attachment layouts, present, and cross-plant transfer stubs) plus Aurelian access/stage flags. These concepts are mapped to Silk.NET Vulkan `ImageLayout`, `AccessFlags`, and `PipelineStageFlags` in a tested mapper rather than copied as Stride-facing state.

The layout tracker is per subresource from M0: each mip/array-layer cell is stored in a flat `mip * arrayLayers + arrayLayer` array, invalid subresources are rejected with diagnostics, no-op transitions produce no plan, and `TransitionAll` emits only real transitions. Buffer transition planning is also pure data: host-write to transfer-read, transfer-write to vertex-read, and transfer-write to shader-read plans expose access/stage facts without recording Vulkan barriers.

A32 deliberately records no `vkCmdPipelineBarrier` calls and introduces no textures, render passes, pipelines, descriptors, swapchains/windows/surfaces, or draws. The likely next step is `A33 — Texture resource M0`, because texture creation can now attach a per-subresource layout tracker without inventing layout semantics inside the texture milestone.

## A33 — Vulkan Texture2D resource M0

Status: implemented.

A33 introduces the first Vulkan image resource for `Aurelian.Graphics`: a focused Texture2D M0 with Aurelian-owned create plans, texture usage and format vocabulary, `VkImage` creation, image memory requirement queries, allocator-backed memory ownership, image-memory binding, optional default color image-view creation, per-subresource layout tracker initialization, and safe disposal.

The milestone deliberately keeps texture data movement and rendering out of scope. Upload staging, `vkCmdCopyBufferToImage`, image barrier command emission, descriptor sets, samplers, render passes, pipelines, draw calls, and swapchain/window/surface integration remain future milestones. A34 should be `Barrier command emission M1` because truthful texture upload and rendering paths need actual image/buffer barrier recording.

## A34 — Barrier command emission M1

A34 implements the first Vulkan command-buffer emission path for the barrier/layout work from A32. The milestone adds an emitter under `Aurelian.Graphics.Vulkan.Resources.Barriers` that lowers texture transition requests to `VkImageMemoryBarrier`, lowers buffer transition requests to `VkBufferMemoryBarrier`, ORs source/destination pipeline stages across all requests, and records one batched `vkCmdPipelineBarrier` through an already-recording `VulkanCommandBufferLease`.

The API keeps pure planning and native handles separated. `VulkanBarrierPlan` and `VulkanBufferTransitionPlan` remain handle-free plan DTOs, while `VulkanTextureBarrierEmission` and `VulkanBufferBarrierEmission` pair those plans with the concrete Vulkan resource only at emission time. The layout tracker mutation discipline remains the A32 discipline: accepted planning mutates tracker state, and A34 emission consumes plans without mutating tracker state a second time. Rollback or submitted-vs-recording reconciliation after failed emission is explicitly deferred.

A34 does not add texture upload, `vkCmdCopyBufferToImage`, render passes, framebuffers, pipelines, descriptors, swapchains, windows, surfaces, VMA/VMASharp, Vortice, or renderer execution. The recommended next milestone is A35 — Texture upload M0, because Vulkan textures, command buffers, staging buffer upload foundations, and barrier emission now exist.


## A35 — Texture upload M0

A35 adds `VulkanTextureUploader` as the first CPU-to-GPU Texture2D data path. The helper is per plant and depends explicitly on `AurelianVulkanPlant`, `IVulkanMemoryAllocator`, `VulkanCommandBufferPool`, and `VulkanFenceBundle`; it owns none of those dependencies and introduces no singleton/service-locator state.

The M0 upload path is synchronous and intentionally narrow:

1. validate the destination texture, plant ownership, `TransferDestination` usage, supported four-byte color format, and exact whole-texture byte size;
2. create a temporary mapped `CpuToGpu` staging buffer with `TransferSource` usage;
3. write CPU RGBA-like bytes through `AurelianVulkanBuffer.Write(...)`;
4. record the destination image transition to `TransferDestination`;
5. record `vkCmdCopyBufferToImage` for mip 0 / array layer 0 / full width-height-depth 1;
6. record the destination image transition to `ShaderResourceFragment`;
7. submit to the plant queue, signal `CommandListFence`, wait for the signal value, retire the command buffer, and only then dispose the staging buffer.

A35 deliberately defers mip generation, partial region uploads, texture arrays/cubes/3D uploads, samplers, descriptor sets, render passes, pipelines, draw commands, swapchains/windows/surfaces, upload rings, async staging retirement, VMA/VMASharp, and Vortice.


## 18. A36 render pass descriptor M0 note

A36 introduces `Aurelian.Graphics.Vulkan.Pipelines.RenderPasses` as the first explicit render-pass boundary. A render pass is described as Aurelian-owned plain data: one M0 color attachment with a `VulkanTextureFormat`, caller-selected `VulkanAttachmentLoadOp` / `VulkanAttachmentStoreOp`, and initial/final `VulkanResourceLayout` values. This deliberately avoids Stride's implicit render pass creation inside pipeline state and avoids hardcoding `Load` for every attachment.

`VulkanRenderPassFactory` validates the descriptor, maps Aurelian format/layout/load-store facts to Silk.NET Vulkan facts, creates a single graphics subpass, and returns an `AurelianVulkanRenderPass` owner that destroys the native `VkRenderPass` idempotently. M0 supports exactly one color attachment; multiple render targets, depth/stencil, MSAA, framebuffer creation, pipeline creation, draw commands, render-pass begin/end command-list integration, and swapchains/windows/surfaces remain deferred. The recommended next milestone is `A37 — Framebuffer M0`, because a native render pass now exists and a framebuffer is the next Vulkan object required before command-list render pass begin/end work.

## A37 framebuffer M0 note

A37 implements the first native framebuffer object over the existing Vulkan render pass and Texture2D resource layers. The M0 descriptor is Aurelian-owned plain data: a framebuffer width, height, and exactly one color attachment texture. Creation validates the plant identity, render pass identity, attachment count, attachment lifetime, dimensions, color-attachment usage, native image-view availability, and render-pass/texture format compatibility before calling `vkCreateFramebuffer`.

This deliberately avoids Stride's hidden command-list-side render pass/framebuffer creation and avoids framebuffer cache complexity before descriptors and compatibility keys stabilize. `AurelianVulkanFramebuffer` owns only the native `VkFramebuffer`; disposing it does not dispose the render pass or attachment textures. Deferred work remains framebuffer caches, command-buffer render pass begin/end, pipelines, draw commands, swapchains/windows/surfaces, depth/stencil, MSAA, MRT, descriptor sets, VMA/VMASharp, and Vortice. Recommended next step: `A38 — Render pass begin/end command M0`.

## A38 render pass begin/end command M0 note

A38 adds the first render-pass boundary command helper in `Aurelian.Graphics`. `VulkanRenderPassCommandEncoder` records `vkCmdBeginRenderPass` and `vkCmdEndRenderPass` into an existing per-plant primary command-buffer lease. The begin request is explicit: an existing `AurelianVulkanRenderPass`, an existing compatible `AurelianVulkanFramebuffer`, and one color clear value. M0 derives the render area from the framebuffer dimensions and uses inline subpass contents.

The command model intentionally remains narrow. The encoder keeps active render-pass state local to the encoder instance, validates that the command buffer is recording, validates plant compatibility, validates render pass/framebuffer lifetime and ownership, rejects framebuffers created for another render pass, rejects double-begin, and rejects end-without-begin. Tests still skip cleanly when Vulkan is unavailable.

Still deferred: graphics pipeline descriptors/state, pipeline binding, draw commands, vertex/index binding, descriptor sets, framebuffer caches, depth/stencil, MSAA, swapchains/windows/surfaces, VMA/VMASharp, and Vortice.


## A39 graphics pipeline descriptor/state M0 note

A39 adds explicit graphics pipeline descriptors and native Vulkan graphics pipeline creation under `Aurelian.Graphics.Vulkan.Pipelines.Graphics`. The M0 descriptor accepts raw per-stage SPIR-V word arrays plus stage metadata, requires exactly one vertex stage and one fragment stage, validates entry points and SPIR-V presence, and supports an optional vertex input model with `Float2`, `Float3`, and `Float4` vertex attributes at vertex input rate. Pipeline state is intentionally narrow: triangle list, fill rasterization, no culling, counter-clockwise front faces, dynamic viewport/scissor, one color attachment with blending disabled, sample count 1, and no depth/stencil.

The native path creates temporary `VkShaderModule` objects, an empty `VkPipelineLayout` with no descriptor set layouts and no push constants, and a `VkPipeline` tied to an explicit existing A36 render pass. Shader modules are destroyed after pipeline creation, while `AurelianVulkanGraphicsPipeline` owns and idempotently destroys the pipeline and layout. This avoids Stride's all-stages-share-one-bytecode constraint, implicit render pass creation inside pipeline state, descriptor set layout magic, unconditional depth bias, and hidden shader compiler coupling.

A39 does not add SDSL-V integration, DXC/Vortice.Dxc, shader/assets dependencies, descriptor sets, uniform buffers, push constants, draw commands, pipeline binding, vertex binding command emission, pipeline cache, swapchain/window/surface, VMA/VMASharp, or Vortice.Vulkan. The intended shader artifact path remains future work: `SDSL-V -> HLSL or Slang -> DXC -> SPIR-V artifact -> Vulkan pipeline creation`; there is no direct SDSL-V-to-SPIR-V plan. Recommended next step: `A40 — DXC subprocess toolchain audit/spike`.


## A40 DXC subprocess toolchain spike note

A40 adds a tooling-only DXC subprocess path under `Aurelian.Shaders.Language.External.Dxc`. The spike uses `Microsoft.Direct3D.DXC` as the package dependency, resolves `AURELIAN_DXC`, packaged DXC content, then PATH, and returns typed unavailable diagnostics rather than failing tests when DXC is absent on the current platform. On this Linux environment the package contained Windows `dxc.exe`/`dxcompiler.dll`/`dxil.dll` files under `build/native/bin/{x64,x86,arm64}` and no Linux `.so`/native `dxc`, so the packaged tool is documented but not invoked here.

The subprocess compiler writes HLSL to a temporary file, invokes DXC with `-spirv`, `-fspv-target-env=vulkan1.3`, `-HV 2021`, `-E`, `-T`, and `-Fo`, captures stdout/stderr/exit code, reads SPIR-V bytes from the output file, and deletes temporary files. Tests cover resolver availability/unavailability, invalid requests, invalid HLSL when a tool is available, and tiny checked-in HLSL vertex/pixel fixtures when DXC is available.

A40 deliberately does not integrate DXC into `Aurelian.Graphics`, does not add `Vortice.Dxc`, does not connect SDSL-V artifact emission to SPIR-V, and does not feed DXC-produced bytes into Vulkan pipeline creation. The intended path remains `SDSL-V -> HLSL/Slang -> DXC subprocess -> SPIR-V artifact -> Vulkan pipeline creation`; direct SDSL-V -> SPIR-V is not planned. Recommended next step: `A41 — HLSL -> SPIR-V shader artifact M0`.


## A41 — HLSL -> SPIR-V shader artifact M0

Status: implemented.

A41 turns the A40 DXC subprocess spike into a shader artifact layer for HLSL stage sources. `Aurelian.Shaders` now accepts typed HLSL vertex, fragment, and compute stage inputs with entry point, profile, and source name metadata; validates stage/profile alignment; invokes DXC only through the existing subprocess wrapper; captures SPIR-V bytes; computes lowercase SHA-256 hashes for UTF-8 HLSL source text and raw SPIR-V bytes; and writes deterministic JSON manifests with ordered fields, diagnostics, DXC arguments, hashes, and base64 SPIR-V payloads.

A41 deliberately remains a tooling/artifact milestone. It does not integrate SDSL-V emission, `Aurelian.Graphics`, `Aurelian.Assets`, Vulkan pipeline creation, Vortice.Dxc, Vortice.Vulkan, runtime DXC invocation, or direct SDSL-V -> SPIR-V generation. If DXC is unavailable, artifact tests assert unavailable diagnostics instead of failing normal test runs. The recommended next milestone is A42 — SDSL-V -> HLSL -> SPIR-V artifact M0.

## A42 — SDSL-V -> HLSL -> SPIR-V shader artifact M0

Status: implemented.

A42 implements the shader/compiler-side SDSL-V to SPIR-V artifact bridge in `Aurelian.Shaders`. The M0 path parses and validates `.sdslv` source, emits traceable HLSL, extracts conventional `VSMain`/`PSMain` stage sources, and reuses the A41 HLSL-to-SPIR-V artifact layer to produce typed stage bytes, hashes, diagnostics, and deterministic JSON when DXC is available. Missing DXC remains an availability diagnostic rather than a normal test failure.

A42 deliberately does not integrate with assets/TOML, Vulkan pipeline creation, swapchains, windows, or runtime graphics. Direct SDSL-V to SPIR-V remains out of scope; A43 should make the graphics pipeline consume existing SPIR-V artifact bytes rather than importing compiler dependencies.

## A43 — Compiled shader stage contract M0

Status: implemented.

A43 bridges A42 shader artifacts to A39 graphics pipeline descriptors without creating a direct shader/graphics dependency. `Aurelian.Rendering.Contracts.Shaders` defines neutral compiled shader DTOs for programs, stages, status, diagnostics, and format versioning. `Aurelian.Shaders.Language.Artifacts.Compiled` exports `SpirvShaderArtifact` and `SdslvSpirvShaderArtifact` into those DTOs after validating artifact success, non-empty stages, duplicate stages, entry points, SPIR-V bytes, and SHA-256 metadata. `Aurelian.Graphics.Vulkan.Pipelines.Graphics.VulkanCompiledShaderStageMapper` consumes only the neutral DTOs and maps vertex/fragment stages to `VulkanShaderStageDescriptor` values after rejecting compute stages for graphics M0 and validating SPIR-V byte shape/magic before little-endian word conversion.

A43 deliberately does not make `Aurelian.Graphics` reference `Aurelian.Shaders`, does not make `Aurelian.Shaders` reference `Aurelian.Graphics`, and does not add DXC/runtime compilation to graphics. Asset/TOML shader integration, descriptor sets, draw commands, swapchains/windows/surfaces, and direct SDSL-V-to-pipeline creation remain deferred. Recommended next milestone: `A44 — Pipeline consumes compiled shader program M0`.

## A44 — Pipeline consumes compiled shader program M0

A44 adds `VulkanCompiledGraphicsPipelineDescriptorFactory` in `Aurelian.Graphics` so graphics code can consume a neutral `CompiledShaderProgram` and produce a `VulkanGraphicsPipelineDescriptor` without requiring a Vulkan runtime. The factory validates the graphics M0 requirements (vertex + fragment stages, no compute stage, no duplicate stage, SPIR-V byte shape through the existing mapper, and valid vertex input descriptors), then forwards the mapped shader stage descriptors plus fixed graphics options into the A39 pipeline descriptor.

A44 also adds an optional helper that accepts an `AurelianVulkanPlant` and `AurelianVulkanRenderPass`, builds the descriptor, and delegates native creation to `VulkanGraphicsPipelineFactory.Create`. Native failures are returned as diagnostics, so Vulkan/runtime availability remains optional for normal tests.

Dependency intent:

- `Aurelian.Graphics` consumes neutral `Aurelian.Rendering.Contracts.Shaders` contracts only.
- `Aurelian.Graphics` does not reference `Aurelian.Shaders` and does not load or invoke DXC/SDSL-V tooling.
- `Aurelian.Shaders` remains the compiler/artifact producer and does not reference `Aurelian.Graphics`.
- Draw commands, pipeline bind commands, descriptor sets, uniforms/push constants, assets/TOML, and swapchain/window/surface remain deferred.

Recommended next milestone: **A45 — Pipeline bind + draw command M0**, because pipelines can now be built from neutral compiled shader programs, but command recording still cannot bind a graphics pipeline or issue a draw.

## A45 — Pipeline bind + vertex draw command M0

Status: implemented.

A45 records the first explicit draw-command shape in `Aurelian.Graphics`: render pass begin returns a typed `VulkanRenderPassScope`, the command buffer lease owns minimal active render-pass state, render pass end validates and clears the supplied scope, and `VulkanDrawCommandEncoder.DrawVertices(...)` validates and records viewport/scissor, graphics pipeline bind, one vertex buffer bind at binding 0/offset 0, and a non-indexed `vkCmdDraw` with instance count 1 and first instance 0.

The milestone remains intentionally pre-presentation and pre-material: there is no swapchain/window/surface, no render-target presentation, no descriptor sets, no uniform buffers, no push constants, no index buffers, no multiple vertex buffers, no pipeline cache, and no `RenderCommandPlan` execution yet. The recommended follow-up is A46 — Valid SPIR-V fixture / first offscreen draw recording proof, so the native success path can be proven without coupling graphics tests to shader compiler projects.

## A46 — Valid SPIR-V fixture / first offscreen draw recording proof

Status: implemented.

A46 proves the first complete offscreen Vulkan draw recording chain in `Aurelian.Graphics.Tests`: plant/device creation, allocator, command buffer pool, fence bundle, color attachment texture, render pass, framebuffer, graphics pipeline, vertex buffer, vertex upload, command buffer begin, render pass begin, non-indexed vertex draw, render pass end, and command buffer end. The proof intentionally stops at command recording; it does not add presentation, swapchains, windows, surfaces, descriptors, uniforms, index buffers, texture sampling, or visual readback.

The milestone also introduces static valid SPIR-V byte fixtures under the graphics tests. Those fixtures are generated once from tiny HLSL triangle shaders and consumed through neutral `Aurelian.Rendering.Contracts.Shaders` compiled shader DTOs, so `Aurelian.Graphics` and `Aurelian.Graphics.Tests` remain free of a runtime shader compiler dependency and do not reference `Aurelian.Shaders`.

Recommended next milestone: A47 — Command submit helper M0, because A46 proves offscreen draw recording but intentionally leaves draw command buffer submission/wait behind a dedicated backend seam.

## A47 — Vulkan command submit helper M0

Status: implemented.

A47 completes the first submit seam after the A46 offscreen recording proof. `Aurelian.Graphics.Vulkan.Commanding.Submit` now owns the one-command-buffer M0 request/result/diagnostic model and submits an executable `VulkanCommandBufferLease` to the plant queue, signals `VulkanFenceBundle.CommandListFence`, optionally waits for the signaled value, retires the lease through `VulkanCommandBufferPool`, and returns typed diagnostics and the signal fence value.

The offscreen draw proof now follows:

```text
record offscreen draw
  -> end command buffer
  -> submit one command buffer
  -> signal/wait command-list timeline fence
  -> retire command buffer
```

No swapchain/window/surface, present/acquire path, render backend abstraction, render graph, descriptor sets, uniforms, index buffers, runtime shader compilation, VMA/VMASharp, or Vortice are introduced. The recommended next milestone is `A48 — Surface/swapchain M0` because offscreen commands can now be recorded and submitted, leaving presentation as the next missing visual path.

## A48 — Surface/swapchain M0

Status: implemented.

A48 introduces the first presentation resources in `Aurelian.Graphics.Vulkan.Presentation`: a Silk.NET.Windowing hidden-window path, `VkSurfaceKHR` ownership, surface support/capability/format/present-mode queries, deterministic format/present-mode selection, `VkSwapchainKHR` creation, swapchain image retrieval, per-image color image views, and idempotent disposal for surface and swapchain owners.

Presentation is explicit at plant creation. `VulkanPlantOptions.EnablePresentation` enables the Silk.NET-required surface instance extensions and requires `VK_KHR_swapchain`; normal offscreen plant creation continues without swapchain requirements. The tests are unavailable/headless-safe and return cleanly when CI cannot create a window, surface, or swapchain.

A48 does not render to swapchain images, does not add a present loop, does not add descriptor/uniform/index-buffer work, does not add shader compiler dependencies to graphics, and does not add VMA/VMASharp or Vortice. Acquire/present methods return deferred diagnostics. Recommended next milestone: **A49 — Swapchain acquire/present M0**.

## A49 — Swapchain acquire/present M0

Status: implemented.

A49 turns the A48 swapchain skeleton into a minimal presentation conveyor belt. `AurelianVulkanSwapchain` now owns one binary image-available semaphore and one binary render-finished semaphore through a tiny `VulkanPresentationSemaphoreSet`, exposes typed acquire and present result models, calls `vkAcquireNextImageKHR`, and calls `vkQueuePresentKHR` for a caller-supplied image index.

Out-of-date, suboptimal, surface-lost, unavailable, disposed, and invalid-index cases are returned as typed results with presentation diagnostics rather than being treated as ordinary exceptions. Tests remain headless-safe: unavailable Vulkan/window/surface/swapchain environments return diagnostics and exit cleanly.

A49 intentionally does not render to swapchain images, does not submit rendering work, does not introduce a compositor, and does not add a present loop. Present M0 uses no wait semaphores because the render-to-swapchain/compositor milestone that will signal the render-finished semaphore is deferred. Recommended next milestone: **A50 — Compositor passthrough M0**.
