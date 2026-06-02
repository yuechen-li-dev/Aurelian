# Aurelian

Aurelian is a greenfield C# engine/runtime project created after the Stri-V rescue effort was paused.

## Status

Aurelian is currently in **pre-MVP bootstrap (A0)**. The repository contains a clean solution skeleton, strict build discipline, architecture notes, and first smoke tests only.

## What Aurelian is

- A greenfield C# engine/runtime.
- A future Dominatus-native runtime spine beginning in A1, after Dominatus is vendored under `vendor/Dominatus/`.
- An explicit data-world engine design.
- A typed lifecycle, actuator-owned side-effect, render-snapshot, and command-plan oriented architecture.
- A test-first codebase with nullable reference types and warnings-as-errors from day one.

## What Aurelian is not

- It is not a Stride runtime fork.
- It does not use the Stride processor architecture as its runtime core.
- It does not use the Stride asset system as its asset foundation.
- It is not editor-first.
- It does not contain renderer, windowing, triangle, asset-pipeline, shader-pipeline, Machina, or Dominatus integration in A0.

## Reference folders

`CodeReferences/*` contains read-only reference material. These folders are not build dependencies, are not part of `Aurelian.slnx`, and must not be compiled, reformatted, linked, or modified as part of Aurelian A0 work.

Current reference folders include:

- `CodeReferences/Machina`
- `CodeReferences/Stride`
- `CodeReferences/WyrmCoil`
- `CodeReferences/oct`

Existing `src/StriV.*` projects are salvage candidates only and are not linked into Aurelian A0.

## Build and test

```bash
dotnet build Aurelian.slnx -c Debug
dotnet test Aurelian.slnx -c Debug
```
