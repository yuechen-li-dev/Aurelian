# Vendor and Reference Strategy

Aurelian separates reference material from buildable dependencies.

## Reference-only folders

`CodeReferences/*` is reference-only material. These folders must not be added to the Aurelian solution, compiled, reformatted, modified, or treated as project dependencies.

Current reference folders include:

- `CodeReferences/Stride` — reference-only Stride and Stri-V-adjacent material.
- `CodeReferences/WyrmCoil` — reference-only WyrmCoil material.
- `CodeReferences/Machina` — reference-only Machina material.
- `CodeReferences/oct` — reference-only material.

Aurelian does not reference `Stride.Graphics`, `Stride.Rendering`, `Stride.Shaders`, Machina, WyrmCoil, oct, or any project under `CodeReferences/`.

## Dominatus vendor boundary

Dominatus is now the first buildable vendored runtime dependency for Aurelian. A1 vendors the minimal Dominatus source needed for the runtime smoke under:

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

Only `Dominatus.Core` and `Dominatus.OptFlow` are linked in `Aurelian.slnx`. `Aurelian.Runtime` references `Dominatus.Core` for the A1 smoke harness. `Aurelian.Core` remains Dominatus-free.

Dominatus was copied from `https://github.com/yuechen-li-dev/Dominatus/` at commit `220df609fc5c4aebca63ed07b953aa13be969ac2`.

## Stri-V salvage boundary

The existing `src/StriV.*` projects are salvage candidates only. They are not linked in Aurelian, not migrated in A1, and not part of `Aurelian.slnx`.

Aurelian core must not take Machina, Stride, WyrmCoil, or Stri-V salvage dependencies. Any future integration must be explicit, phase-scoped, and outside A1.
