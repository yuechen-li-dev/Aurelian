# Vendor and Reference Strategy

Aurelian separates reference material from buildable dependencies.

## Reference-only folders

`CodeReferences/*` is reference-only material. These folders must not be added to the Aurelian solution, compiled, reformatted, modified, or treated as project dependencies.

Current reference folders include:

- `CodeReferences/Stride` — reference-only Stride and Stri-V-adjacent material.
- `CodeReferences/WyrmCoil` — reference-only WyrmCoil material.
- `CodeReferences/Machina` — reference-only Machina material.
- `CodeReferences/oct` — reference-only material.

A0 does not reference `Stride.Graphics`, `Stride.Rendering`, `Stride.Shaders`, Machina, WyrmCoil, oct, or any project under `CodeReferences/`.

## Future Dominatus vendor boundary

Dominatus is not currently vendored as buildable source in this repository. The future actual Dominatus vendor location is:

```text
vendor/Dominatus/
```

A1 will vendor Dominatus there, add buildable Dominatus projects to the solution, and add the first runtime smoke test. A0 deliberately contains no Dominatus project references or package references.

## Stri-V salvage boundary

The existing `src/StriV.*` projects are salvage candidates only. They are not linked in A0, not migrated in A0, and not part of `Aurelian.slnx`.

Aurelian core must not take a Machina dependency. Any future integration must be explicit, phase-scoped, and outside A0.
