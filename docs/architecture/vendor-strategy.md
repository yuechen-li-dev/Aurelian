# Vendor and Reference Strategy

Aurelian separates reference material, vendored buildable dependencies, and Aurelian-owned modules.

## Reference-only folders

`CodeReferences/*` is reference-only material. These folders must not be added to the Aurelian solution, compiled, reformatted, modified, or treated as project dependencies.

Current reference folders include:

- `CodeReferences/Stride` — reference-only Stride and Stri-V-adjacent material.
- `CodeReferences/WyrmCoil` — reference-only WyrmCoil material and the semantic reference for Aurelian SDSL-V.
- `CodeReferences/Machina` — reference-only Machina material.
- `CodeReferences/oct` — reference-only material.

Aurelian does not reference `Stride.Graphics`, `Stride.Rendering`, `Stride.Shaders`, Machina, WyrmCoil, oct, or any project under `CodeReferences/`.

## Dominatus vendor boundary

Dominatus is the first buildable vendored runtime dependency for Aurelian. A1 vendors the minimal Dominatus source needed for the runtime smoke under:

```text
vendor/Dominatus/
```

The A1 vendor subset contains:

```text
vendor/Dominatus/README.md
vendor/Dominatus/LICENSE.txt
vendor/Dominatus/src/Dominatus.Core/
vendor/Dominatus/src/Dominatus.OptFlow/
```

Only `Dominatus.Core` and `Dominatus.OptFlow` are linked in `Aurelian.slnx`. `Aurelian.Runtime` references `Dominatus.Core` for the runtime smoke harness. `Aurelian.Core` remains Dominatus-free.

Dominatus was copied from `https://github.com/yuechen-li-dev/Dominatus/` at commit `220df609fc5c4aebca63ed07b953aa13be969ac2`.

## Aurelian.Shaders module boundary

A3 converted the carried-over `src/StriV.ShaderPipeline/` project identity into the Aurelian-owned `src/Aurelian.Shaders/` module and linked it in `Aurelian.slnx` with matching smoke tests under `tests/Aurelian.Shaders.Tests/`.

A4 added the first WyrmCoil-shaped SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`. A5 began parser convergence for the new AST, and A6 expanded the parser to M1 statements/expressions while adding `docs/architecture/sdslv-compatibility-matrix.md`. The legacy carried-over AST/parser/lowerer and artifact emitter remain temporarily in place and keep their existing behavior until parser, validation, and lowerer convergence milestones replace them deliberately.

WyrmCoil remains reference-only: Aurelian compares and copies language semantics conceptually where useful, not by referencing or compiling WyrmCoil code. Aurelian SDSL-V is its own production C# implementation, and compatibility decisions are tracked explicitly rather than treating WyrmCoil as production truth. `Aurelian.Shaders` must not add project references to `CodeReferences/*`, Stride, Machina, WyrmCoil, Copeland, or the remaining Stri-V salvage projects. Old Stride mixins and old Stride effect/base-shader inheritance are not native Aurelian SDSL-V features.

## Aurelian.Assets and Aurelian.AssetTool module boundary

A11 consumed the remaining carried-over Stri-V asset projects: `src/StriV.AssetPipeline` is now `src/Aurelian.Assets`, and `src/StriV.AssetTool` is now `src/Aurelian.AssetTool`. Both modules are linked in `Aurelian.slnx` with smoke tests under `tests/Aurelian.Assets.Tests/` and `tests/Aurelian.AssetTool.Tests/`.

`Aurelian.Assets` owns early TOML/manifest-based asset orchestration and may call `Aurelian.Shaders` for shader artifact generation. `Aurelian.AssetTool` is a CLI wrapper over Aurelian asset pipeline functionality. This conversion is identity and solution integration only; asset manifest schema convergence and shader asset bridging remain follow-up work.

`Aurelian.Shaders` owns SDSL-V parsing, validation, HLSL emission, artifact contracts, and optional DXC validation. `Aurelian.Assets` must not become a Stride asset system port and must not add Stride, Machina, WyrmCoil, Copeland, or `CodeReferences/*` dependencies.

## Stri-V salvage boundary

The former `src/StriV.ShaderPipeline`, `src/StriV.AssetPipeline`, and `src/StriV.AssetTool` identities have now been consumed by Aurelian modules. Current carried-over code remains migration scaffold where noted, not final Aurelian semantics.

Aurelian core must not take Machina, Stride, WyrmCoil, or Stri-V salvage dependencies. Any future integration must be explicit, phase-scoped, and keep `CodeReferences/*` reference-only.
