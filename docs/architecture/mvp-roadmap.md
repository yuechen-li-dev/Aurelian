# Aurelian MVP Roadmap

- **A0 — Bootstrap:** create the clean solution, strict build discipline, architecture charter, project skeleton, and smoke tests without external runtime/vendor links. **Completed.**
- **A1 — Vendor Dominatus runtime smoke:** vendor Dominatus under `vendor/Dominatus/`, add buildable Dominatus projects to the solution, and add the first runtime smoke while keeping renderer work out of scope. **Completed.**
- **A2 — SDSL-V convergence audit:** establish WyrmCoil Rust SDSL-V as the semantic authority, classify `src/StriV.ShaderPipeline` as migration scaffold, and keep Stride SDSL/mixins as historical reference rather than native Aurelian input. **Completed.**
- **A3 — Aurelian.Shaders identity conversion:** convert the carried-over `StriV.ShaderPipeline` identity into the linked `Aurelian.Shaders` module and add minimal smoke tests while preserving current behavior. **Completed.**
- **A4 — Aurelian.Shaders AST convergence:** added the first WyrmCoil-shaped Aurelian SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`, with tests for paths, type refs, modules, records, streams, enums, shader declarations, expressions, and flow declarations. Legacy parser/lowerer behavior remains untouched. **Completed.**
- **A5 — SDSL-V parser convergence M0:** added the first token-driven SDSL-V parser path under `Aurelian.Shaders.Language`, covering namespace/use/type refs/records/streams/enums/shader shells plus small function/body expression support. Legacy parser/lowerer behavior remains untouched and HLSL emission is unchanged. **Completed.**
- **A6 — SDSL-V parser M1 statements/expressions:** expand the new parser path with broader statement/expression coverage and recovery before validation/emission work.
- **A7 — Actuation contracts:** define typed actuation boundaries and actuator-owned side-effect contracts.
- **A8 — Render snapshot:** define render snapshot contracts independent from a concrete backend.
- **A9 — Command plan:** introduce command-plan generation from snapshots.
- **A10 — Null renderer:** provide a non-windowed renderer implementation for deterministic tests.
- **A11 — First window/backend:** choose and integrate the first window/backend path.
- **A12 — First triangle:** render the first triangle through the established snapshot/command-plan/backend path.

## Shader pipeline status

A3 converted `src/StriV.ShaderPipeline/` to `src/Aurelian.Shaders/` and linked `Aurelian.Shaders` as an Aurelian module in `Aurelian.slnx`. A4 added a separate WyrmCoil-shaped SDSL-V AST contract under `Aurelian.Shaders.Language.Ast`; the carried-over legacy AST/parser/lowerer and artifact emitter remain temporarily in place with preserved behavior.

Aurelian SDSL-V semantics are now represented by the new AST contract, and A5 added the first token-driven parser path under `Aurelian.Shaders.Language.Tokens`, `Aurelian.Shaders.Language.Lexing`, `Aurelian.Shaders.Language.Diagnostics`, and `Aurelian.Shaders.Language.Parsing`. Parser M0 reads namespace/use/type aliases/records/streams/enums/shader shells, material fields, and small function shells/bodies into the new AST. WyrmCoil remains reference-only: semantics are copied conceptually, not referenced as code. A6 should expand parser statement/expression coverage and recovery before AST-backed validation or HLSL emission begins.

`src/StriV.AssetPipeline` and `src/StriV.AssetTool` remain salvage candidates. `CodeReferences/*` remains reference-only and must not be linked, compiled, or modified as part of Aurelian module work.
