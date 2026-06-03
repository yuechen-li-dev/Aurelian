# Aurelian MVP Roadmap

- **A0 — Bootstrap:** create the clean solution, strict build discipline, architecture charter, project skeleton, and smoke tests without external runtime/vendor links. **Completed.**
- **A1 — Vendor Dominatus runtime smoke:** vendor Dominatus under `vendor/Dominatus/`, add buildable Dominatus projects to the solution, and add the first runtime smoke while keeping renderer work out of scope. **Completed.**
- **A2 — SDSL-V convergence audit:** establish WyrmCoil Rust SDSL-V as the semantic authority, classify `src/StriV.ShaderPipeline` as migration scaffold, and keep Stride SDSL/mixins as historical reference rather than native Aurelian input. **Completed.**
- **A3 — Aurelian.Shaders identity conversion:** convert the carried-over `StriV.ShaderPipeline` identity into the linked `Aurelian.Shaders` module and add minimal smoke tests while preserving current behavior. **Completed.**
- **A4 — Aurelian.Shaders AST convergence:** reshape `Aurelian.Shaders` toward the WyrmCoil SDSL-V module/declaration/type model, with tests for records, streams, enums, shader declarations, and type refs. Do not attempt full HLSL emission rewrite unless needed for the AST transition.
- **A5 — Data world M0:** introduce minimal entity/world/component-store contracts, keep Dominatus linked but not over-integrated, produce a world snapshot/query surface, and keep rendering out of scope.
- **A6 — Actuation contracts:** define typed actuation boundaries and actuator-owned side-effect contracts.
- **A7 — Render snapshot:** define render snapshot contracts independent from a concrete backend.
- **A8 — Command plan:** introduce command-plan generation from snapshots.
- **A9 — Null renderer:** provide a non-windowed renderer implementation for deterministic tests.
- **A10 — First window/backend:** choose and integrate the first window/backend path.
- **A11 — First triangle:** render the first triangle through the established snapshot/command-plan/backend path.

## Shader pipeline status

A3 converted `src/StriV.ShaderPipeline/` to `src/Aurelian.Shaders/` and linked `Aurelian.Shaders` as an Aurelian module in `Aurelian.slnx`. This was an identity conversion only: parser, AST, lowering, artifact, mixin/effect, and base-shader behavior remain carried-over scaffold behavior until A4.

Aurelian SDSL-V semantics are not yet converged to WyrmCoil. A4 is the convergence milestone and should replace the old Stri-V/Stride-shaped AST with a WyrmCoil-shaped SDSL-V module/declaration/type model.

`src/StriV.AssetPipeline` and `src/StriV.AssetTool` remain salvage candidates. `CodeReferences/*` remains reference-only and must not be linked, compiled, or modified as part of Aurelian module work.
