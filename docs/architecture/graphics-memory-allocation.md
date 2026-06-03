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
