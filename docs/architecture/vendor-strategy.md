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

This conversion is identity cleanup only. `Aurelian.Shaders` still preserves the carried-over scaffold behavior and has not yet converged parser, AST, lowering, or artifact semantics to WyrmCoil SDSL-V. A4 is responsible for AST convergence toward the WyrmCoil SDSL-V module/declaration/type model.

`Aurelian.Shaders` must not add project references to `CodeReferences/*`, Stride, Machina, WyrmCoil, Copeland, or the remaining Stri-V salvage projects.

## Stri-V salvage boundary

`src/StriV.AssetPipeline` and `src/StriV.AssetTool` remain salvage candidates only. They are not linked in Aurelian, not migrated in A3, and not part of `Aurelian.slnx`.

The former `src/StriV.ShaderPipeline` identity has been consumed by `src/Aurelian.Shaders`; its current code is migration scaffold, not final Aurelian SDSL-V semantics.

Aurelian core must not take Machina, Stride, WyrmCoil, or Stri-V salvage dependencies. Any future integration must be explicit, phase-scoped, and keep `CodeReferences/*` reference-only.
