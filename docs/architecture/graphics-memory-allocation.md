# Aurelian Graphics Memory Allocation Strategy

## 1. Purpose

Vulkan buffers and textures cannot be implemented responsibly until Aurelian has a memory allocation strategy. Buffer resource M0 will need device memory, staging paths, upload retirement, and telemetry immediately; texture M0 will add image memory, layout-aware upload staging, and larger pressure spikes. The allocator choice therefore affects buffers, textures, upload rings, staging buffers, fence retirement, telemetry, future descriptor/resource pressure accounting, and later Dominatus utility decisions.

A27 makes the strategy decision before resource M0. The conclusion is intentionally not "bake VMA into the architecture." VMA or VMASharp may become the best low-level backend, but Aurelian must own the contracts and policy seams: allocation requests/results, budget facts, telemetry, retirement/fence lifecycle, grow/shrink rules, and Dominatus policy hooks.

## 2. Core rule

```text
Aurelian owns allocation policy. Allocator libraries provide bounded allocation mechanics.
```

This means allocation behavior must be observable and replaceable at the Aurelian boundary. A future VMA backend is allowed, but higher graphics code should depend on Aurelian contracts, not VMA types or VMA policy assumptions.

## 3. Allocation layers

Aurelian should treat graphics memory as three explicit layers.

```text
Aurelian allocation contracts:
  BufferCreatePlan
  TextureCreatePlan
  AllocationRequest
  AllocationResult
  GpuResourceState
  PlantId
  fence retirement
  telemetry

Allocator backend:
  raw Vulkan allocator
  VMA/VMASharp allocator
  future custom arena allocator

Policy/controller:
  Dominatus observes telemetry
  decides grow/shrink/reuse/defrag/upload-budget behavior
```

### Aurelian allocation contracts

Contracts are the stable architecture surface. Buffer and texture creation plans describe intended usage, size, format, memory preference, mapping needs, and plant ownership. Allocation requests/results carry the exact facts needed by buffers/textures without leaking backend implementation types. `GpuResourceState` carries `PlantId`, resource lifecycle state, fence-retirement facts, and layout/state facts needed by later barrier code. Telemetry is plain data so tests, diagnostics, and future Dominatus actuators can consume it without depending on Vulkan objects.

### Allocator backend

The backend performs bounded allocation mechanics for one plant/device. Initial candidates are a small raw Vulkan allocator, a VMA/VMASharp allocator, or a future custom arena allocator. Backends must be swappable behind `IVulkanMemoryAllocator`, must not be global, and must not share allocations across plants.

### Policy/controller

Policy decides what should happen over time: when to grow arenas, shrink arenas, reuse staging, defer uploads, or invoke compaction/defragmentation if supported by the backend. Dominatus should observe allocation telemetry and budget facts rather than directly owning Vulkan memory handles.

## 4. VMA / VMASharp evaluation

### Why VMA is attractive

VMA is attractive as backend plumbing because it addresses known Vulkan allocation hazards:

- avoids one `vkAllocateMemory` call per resource;
- handles memory type selection;
- suballocates from larger memory blocks;
- supports persistently mapped or mapped-on-demand allocations;
- is widely used in Vulkan engines and examples;
- fits the per-device/per-plant shape because each plant can own its own allocator instance.

### Risks

VMA should still not become the architecture boundary:

- package/API maturity for the specific .NET binding must be verified;
- NativeAOT compatibility is unknown until tested in this repository;
- telemetry and budget facts may be hidden if the wrapper is too thin or too leaky;
- a poorly placed wrapper could constrain Dominatus utility allocation policy;
- allocator instances must be per-plant, never process-global;
- VMA types crossing above `Aurelian.Graphics.Vulkan.Resources` would couple buffers, textures, and policy to one backend.

### Recommendation

Use VMA/VMASharp only behind an Aurelian allocator interface. Do not expose VMA types above `Aurelian.Graphics.Vulkan.Resources`. If VMASharp is verified during implementation, it can become a backend implementation without changing buffer/texture contracts. If verification is blocked, Aurelian should still proceed with the allocator boundary and a narrow raw Vulkan M0 backend.

## 5. Raw Vulkan allocator fallback

A raw Vulkan allocator is acceptable for smoke/prototype work only if it is isolated behind the same `IVulkanMemoryAllocator` boundary. It should not spread through buffer or texture code and must not become the long-term allocator by accident.

The raw fallback can be useful when VMA package/API/NativeAOT verification blocks progress, and it can support tiny tests or first buffer creation. Even then it must:

- cache physical device memory properties at plant/device initialization or allocator construction;
- centralize memory type selection;
- centralize `vkAllocateMemory` and bind calls;
- report allocation telemetry as plain data;
- tag allocations with `PlantId`;
- retire/free allocations through explicit fence lifecycle rules;
- remain replaceable by a VMA backend.

## 6. Dominatus utility allocation policy

A27 does not implement Dominatus allocation policy. It preserves the seam so policy can be added without rewriting resources.

Future Dominatus policy should observe per-plant facts such as:

- allocation count, requested bytes, committed bytes, live bytes, and retired bytes;
- upload ring pressure;
- staging buffer pressure;
- descriptor pressure;
- memory budget/usage if available from Vulkan extensions or backend telemetry;
- high-water marks;
- fence-retired bytes;
- grow/shrink cooldown state;
- allocation rejection/fallback reasons;
- backend-specific defrag/compaction opportunity if exposed safely.

Dominatus utility decisions can then choose to:

- grow an arena;
- shrink an arena;
- defer an upload;
- reuse staging memory;
- compact/defrag if the backend supports it;
- switch strategy under pressure;
- prefer lower-utility uploads/resources when memory budget is constrained.

The important seam is that Dominatus consumes Aurelian telemetry and emits Aurelian policy decisions. It does not depend on VMA objects, raw Vulkan memory handles, or hidden backend state.

## 7. Recommended M0 allocator architecture

Future implementation should introduce the allocation boundary before adding buffer/texture resource code.

```text
Aurelian.Graphics/Vulkan/Resources/
  GpuResourceState
  VulkanAllocationRequest
  VulkanAllocationResult
  IVulkanMemoryAllocator
  VulkanMemoryAllocation
  VulkanMemoryAllocatorTelemetry
  RawVulkanMemoryAllocator
  VmaVulkanMemoryAllocator // later if package chosen
```

Expected responsibilities:

- buffers and textures depend on `IVulkanMemoryAllocator`, not raw `vkAllocateMemory` or VMA directly;
- allocator implementation is selected per plant/device;
- every allocation carries `PlantId`;
- allocation handles are Aurelian-owned wrapper values/classes;
- telemetry is plain data suitable for diagnostics, tests, and future Dominatus observation;
- resource retirement accepts fence values from the A25/A26 fence/pool foundation;
- raw Vulkan and VMA backends satisfy the same request/result contract.

## 8. Recommendation for A28/A27 implementation

The next implementation step should be allocator contracts plus a raw Vulkan M0 backend first, then VMA backend later. In milestone terms:

```text
A28 — Vulkan allocator contracts + raw allocator M0
```

This recommendation is conditional on current package evidence: no VMASharp package/API was present in the repo or local NuGet cache during A27 inspection. Exact VMASharp API and NativeAOT verification is therefore deferred. If VMASharp is easy to verify during A28 without expanding scope, it may be added as a backend implementation, but VMA must not be exposed above the allocator boundary.

Buffer resource M0 should follow only after this boundary exists, or it should include the boundary as its first step before creating actual buffer resources.

## 9. Do-not-carry-over checklist

Do not carry these patterns into Aurelian:

- no per-resource memory properties query;
- no per-resource `vkAllocateMemory` spread across buffers/textures;
- no upload buffer unbounded growth;
- no resource lifetime tied to command-list object references;
- no global allocator;
- no cross-plant allocation sharing;
- no hidden VMA types in higher layers;
- no buffer/texture code that owns allocator policy directly;
- no Dominatus policy coupled to backend-specific allocation handles;
- no allocator telemetry that requires polling opaque backend internals from unrelated systems.

## 10. A28 implementation note

A28 implements the first version of the allocator boundary described above. `Aurelian.Graphics.Vulkan.Resources.Allocation` now contains Aurelian-owned allocation requests/results, allocation handles, `GpuResourceState`, status/diagnostic contracts, allocator telemetry, `IVulkanMemoryAllocator`, and a narrow `RawVulkanMemoryAllocator` backend.

The raw backend is intentionally M0 fallback plumbing. It allocates one `VkDeviceMemory` object per successful request only so the next resource milestone can prove buffer ownership against a real allocator contract. Future buffer and texture code must depend on `IVulkanMemoryAllocator`; it must not call `vkAllocateMemory`/`vkFreeMemory` directly and must not expose backend-specific VMA or raw Vulkan allocation details.

VMA/VMASharp remains deferred. A future VMA backend may replace or supplement the raw backend behind the same contracts after package/API and NativeAOT behavior are verified.


## 11. A29 buffer resource note

A29 implements the first resource that consumes the A28 allocation boundary: `Aurelian.Graphics.Vulkan.Resources.Buffers`. Buffer creation owns Aurelian usage flags and create plans, creates `VkBuffer`, queries `VkMemoryRequirements`, requests allocation through `IVulkanMemoryAllocator`, binds the returned allocation with `vkBindBufferMemory`, and exposes `GpuResourceState` tagged with `PlantId` and allocation backend.

The allocator boundary is preserved deliberately:

- buffer code does not call `vkAllocateMemory` or `vkFreeMemory`;
- raw allocation/free remains isolated to `RawVulkanMemoryAllocator`;
- allocation request size uses Vulkan memory requirements size, while buffer resource state records the logical plan size;
- allocation handles own backend size/offset/memory facts and are disposed by the resource;
- future VMA/arena allocators can satisfy the same request/result contract without changing buffer creation contracts.

A29 intentionally defers mapped memory, upload rings, staging/copy commands, descriptor binding, textures, render passes, pipelines, and draw paths. The recommended A30 step is Buffer mapped memory / upload M0 so data movement is explicit before higher-level rendering milestones require populated buffers.

## 12. A30 mapped memory / CPU upload M0 note

A30 extends the A28/A29 allocation boundary with a narrow mapped-memory contract. `VulkanAllocationRequest.MapOnCreate` allows allocator backends to persistently map allocations when the requested usage is host-visible (`CpuToGpu` or `GpuToCpu`). `GpuOnly + MapOnCreate` is rejected at the allocator boundary with an explicit diagnostic, and raw `vkMapMemory`/`vkUnmapMemory` calls remain isolated to `RawVulkanMemoryAllocator`.

`VulkanMemoryAllocation` records whether an allocation is mapped and writable without exposing the mapped pointer publicly. `AurelianVulkanBuffer.Write(ReadOnlySpan<byte>, ulong)` is the public CPU upload M0 path for buffers: it rejects disposed buffers, unmapped/non-writable allocations, and out-of-bounds writes, while empty writes are successful no-ops. The M0 allocator selects `HOST_VISIBLE | HOST_COHERENT` memory for `CpuToGpu`/`GpuToCpu`, so no flush/invalidate path is implemented yet.

Still deferred:

- staging-to-device-local buffer copies;
- command buffer recording/submission for uploads;
- upload ring and Dominatus upload-budget policy;
- non-coherent memory flush/invalidate support;
- textures/images and image layout transitions;
- descriptor binding, render passes, pipelines, and draw paths.

The recommended next step is `A31 — Staging buffer / device-local upload copy M0` so device-local buffers can be populated without weakening the mapped-allocation boundary.


## 13. A31 staging buffer / device-local upload copy M0 note

A31 adds the first device-local buffer population path while preserving the allocator contract from A28-A30. `Aurelian.Graphics.Vulkan.Resources.Uploads.VulkanBufferUploader` is per plant and depends on the plant, `IVulkanMemoryAllocator`, `VulkanCommandBufferPool`, and `VulkanFenceBundle`; it does not own those dependencies and introduces no global state.

The M0 upload path is deliberately one-shot and synchronous:

1. validate the destination plant, `TransferDestination` usage, non-empty data, and bounds;
2. create a temporary mapped `CpuToGpu` staging buffer with `TransferSource` usage through `VulkanBufferFactory`;
3. write bytes through `AurelianVulkanBuffer.Write(...)`;
4. record `vkCmdCopyBuffer` into a command-buffer lease;
5. submit to the plant queue and signal `CommandListFence`;
6. wait for that timeline value before retiring the command buffer and disposing the staging buffer.

This intentionally avoids unsafe staging lifetime: temporary staging memory is not freed until the GPU copy has completed. It also keeps raw allocation and mapping APIs isolated to allocator backends. Deferred work remains upload rings, batching, persistent staging pools, async/fence-retired staging, texture upload, barriers/layout tracking, non-coherent flush/invalidate policy, descriptors, swapchains/windows/surfaces, render passes, pipelines, and draws.

## 14. A32 barrier/layout tracker M0 note

A32 adds the resource-state transition vocabulary that sits beside, not inside, the allocation boundary. `Aurelian.Graphics.Vulkan.Resources.Barriers` defines Aurelian resource layouts, access flags, stage flags, Vulkan mapping facts, pure image barrier plans/batches, a per-subresource layout tracker, and buffer access transition plans. This keeps memory ownership (`VulkanMemoryAllocation` and `GpuResourceState`) separate from synchronization/layout planning while giving future buffer, texture, upload, and draw paths a common state vocabulary.

The layout tracker is per mip and array layer from the start, avoiding the shared mutable `NativeLayout`/ignored-subresource pitfalls identified in the Stride reference audit. `TransitionAll` emits only subresources that actually change. Cross-plant transfer layouts are present as explicit stubs and map as transfer source/destination for M0 while queue-family ownership transfer planning remains deferred. No command emission, textures, render passes, pipelines, descriptors, swapchains/windows/surfaces, VMA/VMASharp, or Vortice work is introduced by A32.
