# Aurelian

Aurelian is a greenfield C# engine/runtime project created after the Stri-V rescue effort was paused.

## Status

Aurelian is currently in pre-MVP module bring-up. The repository contains a clean solution skeleton, strict build discipline, architecture notes, smoke tests, the first vendored runtime dependency (Dominatus), and the newly linked `Aurelian.Shaders` module.

A3 converted the carried-over `StriV.ShaderPipeline` project identity into `Aurelian.Shaders`. This is an identity conversion only: shader parser, AST, lowering, artifact, mixin/effect, and base-shader behavior have not yet converged to WyrmCoil SDSL-V semantics. A4 is the planned AST convergence milestone toward WyrmCoil SDSL-V.

## What Aurelian is

- A greenfield C# engine/runtime.
- A Dominatus-native runtime spine beginning in A1, with Dominatus vendored under `vendor/Dominatus/`.
- An explicit data-world engine design.
- A typed lifecycle, actuator-owned side-effect, render-snapshot, and command-plan oriented architecture.
- A test-first codebase with nullable reference types and warnings-as-errors for Aurelian-owned projects.
- The home of `Aurelian.Shaders`, an Aurelian-owned shader module that currently preserves carried-over scaffold behavior pending A4 SDSL-V AST convergence.

## What Aurelian is not

- It is not a Stride runtime fork.
- It does not use the Stride processor architecture as its runtime core.
- It does not use the Stride asset system as its asset foundation.
- It is not editor-first.
- It does not contain renderer, windowing, triangle, asset-pipeline, Machina integration, WyrmCoil integration, or remaining Stri-V salvage integration.
- It does not yet implement WyrmCoil-converged Aurelian SDSL-V semantics.

## Vendored runtime dependency

A1 vendors the minimal Dominatus source needed for the runtime smoke under:

```text
vendor/Dominatus/
```

`Aurelian.slnx` links only `vendor/Dominatus/src/Dominatus.Core/Dominatus.Core.csproj` and `vendor/Dominatus/src/Dominatus.OptFlow/Dominatus.OptFlow.csproj`. `Aurelian.Runtime` references `Dominatus.Core`; `Aurelian.Core` remains Dominatus-free.

## Shader module

A3 links `src/Aurelian.Shaders/Aurelian.Shaders.csproj` into `Aurelian.slnx` with smoke tests under `tests/Aurelian.Shaders.Tests/`. The module was renamed from the carried-over `StriV.ShaderPipeline` project, with namespace and project metadata updated to `Aurelian.Shaders`.

A4 should converge the `Aurelian.Shaders` AST toward the WyrmCoil SDSL-V module/declaration/type model. It should not start renderer/HAL work, asset pipeline migration, Stride SDSL compatibility, or full HLSL emission rewrite unless necessary for the AST transition.

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
