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
- `Aurelian.World` must not depend on rendering, assets, shaders, Dominatus, physics, navigation, or UI.
- `Aurelian.Actuation` may depend on world contracts for world mutation.
- `Aurelian.Runtime` integrates Dominatus and dispatch.
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
- Render backend can later use Silk.NET/Vortice behind `Aurelian.Rendering.*`.
- Physics can later use BEPU behind `Aurelian.Physics.*`.
- Navigation can later use DotRecast behind `Aurelian.Navigation.*`.
- Assets can keep Tomlyn because TOML syntax is not strategic architecture.
- Shader DXC validation remains optional and external.
