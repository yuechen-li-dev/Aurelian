# Aurelian

Aurelian is a greenfield C# engine/runtime project created after the Stri-V rescue effort was paused.

## Status

Aurelian is currently in pre-MVP module bring-up. The repository contains a clean solution skeleton, strict build discipline, architecture notes, smoke tests, the first vendored runtime dependency (Dominatus), and the linked `Aurelian.Shaders` module.

A3 converted the carried-over `StriV.ShaderPipeline` project identity into `Aurelian.Shaders`. A4 added the first WyrmCoil-shaped SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`. A5 added the first token-driven SDSL-V parser M0 path under `Aurelian.Shaders.Language`. A6 expanded the new parser to M1 statement/expression syntax and added an explicit WyrmCoil ↔ Aurelian SDSL-V compatibility matrix. A7 added validation M0, A8 added HLSL emission M0, A9 added a new `Aurelian.Shaders.Language.Artifacts` manifest M0 over the source -> parse -> validate -> HLSL emit -> artifact/manifest path, and A10 added optional DXC validation M0 for generated HLSL artifacts when a DXC executable is available, while the legacy carried-over shader parser, AST, lowerer, artifact emitter, mixin/effect, and base-shader behavior remain temporarily in place.

## What Aurelian is

- A greenfield C# engine/runtime.
- A Dominatus-native runtime spine beginning in A1, with Dominatus vendored under `vendor/Dominatus/`.
- An explicit data-world engine design.
- A typed lifecycle, actuator-owned side-effect, render-snapshot, and command-plan oriented architecture.
- A test-first codebase with nullable reference types and warnings-as-errors for Aurelian-owned projects.
- The home of `Aurelian.Shaders`, an Aurelian-owned shader module with a new WyrmCoil-inspired SDSL-V AST contract, token-driven parser/validator/HLSL emitter paths, a deterministic SDSL-V artifact manifest M0, optional DXC validation M0, an explicit compatibility matrix, and a temporarily preserved carried-over legacy pipeline.

## What Aurelian is not

- It is not a Stride runtime fork.
- It does not use the Stride processor architecture as its runtime core.
- It does not use the Stride asset system as its asset foundation.
- It is not editor-first.
- It does not contain renderer, windowing, triangle, asset-pipeline, Machina integration, WyrmCoil integration, or remaining Stri-V salvage integration.
- It does not reference or compile WyrmCoil code; WyrmCoil remains reference-only, with SDSL-V semantics compared through an explicit compatibility matrix rather than blindly copied into production code.

## Vendored runtime dependency

A1 vendors the minimal Dominatus source needed for the runtime smoke under:

```text
vendor/Dominatus/
```

`Aurelian.slnx` links only `vendor/Dominatus/src/Dominatus.Core/Dominatus.Core.csproj` and `vendor/Dominatus/src/Dominatus.OptFlow/Dominatus.OptFlow.csproj`. `Aurelian.Runtime` references `Dominatus.Core`; `Aurelian.Core` remains Dominatus-free.

## Shader module

A3 links `src/Aurelian.Shaders/Aurelian.Shaders.csproj` into `Aurelian.slnx` with smoke tests under `tests/Aurelian.Shaders.Tests/`. The module was renamed from the carried-over `StriV.ShaderPipeline` project, with namespace and project metadata updated to `Aurelian.Shaders`.

A4 converged the first `Aurelian.Shaders` AST contract toward the WyrmCoil SDSL-V module/declaration/type model in `Aurelian.Shaders.Language.Ast`. A5 added the first token-driven parser M0 path under `Aurelian.Shaders.Language`, including new token, lexer, diagnostic, and parser namespaces. A6 expanded that path to parser M1 statements/expressions: precedence, postfix chains, array literals, with expressions, switch/match parse shapes, fallibility parse shapes, if/else, for, assignment, expression, empty, let, and return statements. A7 adds structural validation M0 under `Aurelian.Shaders.Language.Validation`, with stable validation diagnostics for duplicate declarations/uses/fields/variants/generics/shader members/locals, unknown type references, and invalid array lengths. A8 adds HLSL emission M0 under `Aurelian.Shaders.Language.Emission.Hlsl` over the new parser/AST/validator path, with deterministic output for record/stream structs, shader stage functions, basic statements, and basic expressions. A9 adds artifact manifest M0 under `Aurelian.Shaders.Language.Artifacts`: `SdslvShaderArtifactEmitter` packages source identity, SHA-256 source hash, emitted HLSL, combined parse/validation/emission diagnostics, success state, and heuristic stage/profile entry point metadata into deterministic in-memory artifacts and JSON manifests. A10 adds optional DXC validation M0 under `Aurelian.Shaders.Language.External.Dxc`: `DxcValidator` can validate generated HLSL syntax/profile/entry point combinations one stage at a time when DXC is available, and `DxcDiscovery` checks `AURELIAN_DXC`, then `dxc`, then `dxc.exe` on `PATH`. The checked-in smoke fixture at `tests/Aurelian.Shaders.Tests/Fixtures/Sdslv/smoke_triangle.sdslv` is parsed, validated, emitted, and packaged by xUnit tests so the original SDSL-V source remains inspectable. Validation/emission/artifact/DXC M0 are not full expression type checking, interface satisfaction, SPIR-V generation, renderer/window integration, asset/TOML pipeline integration, or shader behavior evaluation. DXC is external validation only: normal builds/tests do not require DXC, Vulkan SDK is not required, and missing DXC produces skip results rather than failures. Manifest JSON includes HLSL for M0 simplicity and may split HLSL into separate output files later. Stage/profile inference is heuristic in M0. The WyrmCoil ↔ Aurelian compatibility matrix lives in `docs/architecture/sdslv-compatibility-matrix.md`. Aurelian SDSL-V remains its own C# production implementation; WyrmCoil remains reference-only, and old Stride mixins/effect/base-shader compatibility are not native language features. Unsupported HLSL M0 constructs report emission diagnostics instead of using DXC or a renderer. `.sdslvtest` files are future work: Aurelian may eventually support them analogously to `.octest` by porting the Oct interpreter architecture to C# and using a CPU evaluator for deterministic shader behavior tests, but that is intentionally deferred until parser, validation, emission, and artifact contracts are stable.

## Reference folders

`CodeReferences/*` contains read-only reference material. These folders are not build dependencies, are not part of `Aurelian.slnx`, and must not be compiled, reformatted, linked, or modified as part of Aurelian work.

Current reference folders include:

- `CodeReferences/Machina`
- `CodeReferences/Stride`
- `CodeReferences/WyrmCoil`
- `CodeReferences/oct`

`src/StriV.AssetPipeline` and `src/StriV.AssetTool` remain salvage candidates only and are not linked into Aurelian.

## Build and test

```bash
dotnet build Aurelian.slnx -c Debug
dotnet test Aurelian.slnx -c Debug
```
