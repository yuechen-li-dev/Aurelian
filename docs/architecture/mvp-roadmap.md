# Aurelian MVP Roadmap

- **A0 — Bootstrap:** create the clean solution, strict build discipline, architecture charter, project skeleton, and smoke tests without external runtime/vendor links. **Completed.**
- **A1 — Vendor Dominatus runtime smoke:** vendor Dominatus under `vendor/Dominatus/`, add buildable Dominatus projects to the solution, and add the first runtime smoke while keeping renderer work out of scope. **Completed.**
- **A2 — SDSL-V convergence audit:** establish WyrmCoil Rust SDSL-V as the semantic authority, classify `src/StriV.ShaderPipeline` as migration scaffold, and keep Stride SDSL/mixins as historical reference rather than native Aurelian input. **Completed.**
- **A3 — Aurelian.Shaders identity conversion:** convert the carried-over `StriV.ShaderPipeline` identity into the linked `Aurelian.Shaders` module and add minimal smoke tests while preserving current behavior. **Completed.**
- **A4 — Aurelian.Shaders AST convergence:** added the first WyrmCoil-shaped Aurelian SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`, with tests for paths, type refs, modules, records, streams, enums, shader declarations, expressions, and flow declarations. Legacy parser/lowerer behavior remains untouched. **Completed.**
- **A5 — SDSL-V parser convergence M0:** add a token-driven parser path for the new AST, initially covering namespace/use/type refs/records/streams/enums/shader shells, while keeping the legacy parser/lowerer intact and avoiding HLSL emission changes.
- **A6 — Actuation contracts:** define typed actuation boundaries and actuator-owned side-effect contracts.
- **A7 — Render snapshot:** define render snapshot contracts independent from a concrete backend.
- **A8 — Command plan:** introduce command-plan generation from snapshots.
- **A9 — Null renderer:** provide a non-windowed renderer implementation for deterministic tests.
- **A10 — First window/backend:** choose and integrate the first window/backend path.
- **A11 — First triangle:** render the first triangle through the established snapshot/command-plan/backend path.

## Shader pipeline status

A3 converted `src/StriV.ShaderPipeline/` to `src/Aurelian.Shaders/` and linked `Aurelian.Shaders` as an Aurelian module in `Aurelian.slnx`. A4 added a separate WyrmCoil-shaped SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`; the carried-over legacy AST/parser/lowerer and artifact emitter remain temporarily in place with preserved behavior.

Aurelian SDSL-V semantics are now represented by the new AST contract, but parser convergence has not started. A5 will begin parser convergence against the new model. WyrmCoil remains reference-only: semantics are copied conceptually, not referenced as code.

`src/StriV.AssetPipeline` and `src/StriV.AssetTool` remain salvage candidates. `CodeReferences/*` remains reference-only and must not be linked, compiled, or modified as part of Aurelian module work.
