# SDSL-V Compatibility Matrix

## 1. Purpose

WyrmCoil SDSL-V remains the reference and semantic inspiration for Aurelian SDSL-V, especially for the Oct-shaped module/declaration/statement/expression/validation direction established during A2 through A8.

Aurelian SDSL-V is now its own production C# implementation under `Aurelian.Shaders.Language`. The matrix below tracks where Aurelian currently matches WyrmCoil, where it intentionally differs, where features are deferred, and where WyrmCoil concepts are not applicable to Aurelian.

This document exists to avoid accidental semantic drift while also avoiding blind copying of prototype constraints that do not fit Aurelian production needs.

## 2. Status legend

- **Compatible** — Aurelian has the same practical syntax or AST intent as WyrmCoil for the current milestone.
- **Intentionally different** — Aurelian has made a deliberate syntax, API, implementation, or scope choice.
- **Deferred** — The feature is acknowledged but not implemented in the production Aurelian path yet.
- **Not applicable** — The feature is outside Aurelian's native SDSL-V model.
- **Unknown** — Compatibility needs more investigation before a decision is recorded.

## 3. Feature matrix

| Feature | WyrmCoil SDSL-V | Aurelian SDSL-V | Status | Notes |
| ------- | --------------- | --------------- | ------ | ----- |
| File/module structure | Rust reference parser produces a module with namespace, uses, and declarations. | C# parser produces `SdslvModule` with namespace, uses, and declarations. | Compatible | Production implementation is independent C# code. |
| Namespace | `namespace Path;`. | `namespace Path;`. | Compatible | Parsed in M0. |
| Use imports | `use Path;`. | `use Path;`. | Compatible | Parsed in M0. |
| Path/type refs | Dotted paths and array type refs. | Dotted paths and `array<T, N>`. | Compatible | Parsed in M0. |
| Arrays | Array type refs and array literals. | Array type refs and M1 array literals. | Compatible | Literal syntax is `[a, b]` and `[]`. |
| Records | Record field blocks. | Record field blocks. | Compatible | Parsed in M0. |
| Streams | Stream field blocks. | Stream field blocks. | Compatible | Parsed in M0. |
| Enums | Enum declarations and variant match arms. | Enum declarations and variant match arms. | Compatible | A7 validation reports duplicate variants. |
| Interfaces | Supported in WyrmCoil reference. | AST exists, parser emits unsupported diagnostics for declarations. | Deferred | Parser M2 or validation phase should decide scope. |
| Shaders | Shader declarations. | Shader declarations. | Compatible | Parsed in M0 shell form. |
| Material fields | Shader material fields. | `material Name: Type;` and `material { ... }`. | Compatible | Parsed in M0. |
| Shader generics | Generic parameters. | Generic parameters. | Compatible | Parsed in M0. |
| Implements | Implements list. | Implements list. | Compatible | Parsed in M0. |
| Where constraints | Generic constraints. | Generic constraints. | Compatible | Parsed in M0. |
| Compile declarations | Supported in WyrmCoil reference. | Unsupported diagnostic in parser M1. | Deferred | No artifact generation over new AST yet. |
| Functions | Function declarations with bodies. | Function declarations with bodies. | Compatible | Parser shell M0, statement/expression M1. |
| Stage functions | Stage methods. | Stage methods. | Compatible | Parsed in M0 with M1 body parsing. |
| Statements | Let, assignment, return, if/else, for, expression, empty statements. | Let, assignment, return, if/else, bounded `for i in start..end step n`, expression, empty statements. | Compatible | M1 parse shape only; validation is deferred. |
| Expressions | Reference precedence/postfix/body subset plus advanced forms. | M1 precedence, postfix, arrays, with, switch, match, try/unwrap parse shapes. | Compatible | Utility expressions remain deferred. |
| Field access | `foo.bar`. | `foo.bar`. | Compatible | Chaining supported in M1. |
| Indexing | `array[index]`. | `array[index]`. | Compatible | Chaining supported in M1. |
| Calls | `callee(args)`. | `callee(args)`. | Compatible | Chaining supported in M1. |
| Binary/unary operators | Arithmetic/comparison subset in reference, with additional operators tracked over time. | `||`, `&&`, `==`, `!=`, `<`, `<=`, `>`, `>=`, `+`, `-`, `*`, `/`, `%`, unary `!`, unary `-`. | Intentionally different | Aurelian M1 hardens logical operators and modulo in the C# parser now. |
| Array literals | `[a, b]`, `[]`. | `[a, b]`, `[]`. | Compatible | Parsed in M1. |
| With expressions | Postfix `base with { Field: value }` in WyrmCoil. | Postfix `base with { Field = value; }`; colon is also accepted. | Intentionally different | Aurelian documents assignment-style updates as preferred C# production syntax. |
| Switch | `switch { case cond => value else => value }` and subject switch. | `switch x { case cond => value; else => value; }`; `->` also accepted. | Compatible | Semicolon/comma separators are accepted in Aurelian M1. |
| Match | Enum variant arms plus fallible `ok`/`err` arms in reference. | Enum variant arms, `else`, and fallible `ok`/`err` arm parse shapes. | Intentionally different | Aurelian adds an explicit else-arm AST shape for parse completeness. |
| Utility expressions | `when utility { case value when guard score score else value }` with options in reference. | AST exists; parser M1 defers utility expression parsing. | Deferred | Parser M2 should revisit syntax and options. |
| Flow declarations | Supported in WyrmCoil reference. | AST exists; parser emits unsupported diagnostics. | Deferred | Kept out of parser M1. |
| Board fields | Flow board model in reference. | AST exists only. | Deferred | No parser/validation support yet. |
| States | Flow state model in reference. | AST exists only. | Deferred | No parser/validation support yet. |
| When/goto | Flow control in reference. | AST support exists for flow statements only. | Deferred | No parser support in M1. |
| Fallibility/try/unwrap | Reference supports postfix `?`/`!` and fallible match arms. | M1 supports prefix `try expr`, prefix `unwrap expr`, and postfix `?`/`!`. | Intentionally different | Prefix forms are Aurelian's documented M1 syntax; postfix forms are retained as compatibility parse shapes. |
| Diagnostics | Reference parser/validator/emitter diagnostics. | C# lex/parse/validation/emission diagnostics returned in result objects. | Compatible | A7 adds stable validation codes `SV1001`, `SV1002`, `SV1101`, `SV1201`, `SV1301`, `SV1302`, `SV1401`, `SV1402`, `SV1501`, and `SV1901`; A8 adds HLSL M0 emission codes `SV3001` through `SV3004` for unsupported declarations/statements/expressions/types. |
| Validation | Reference includes validation. | A7 structural validation M0 over the new AST is implemented. | Compatible | Covers duplicates, basic type-reference validity, positive array lengths, and same-scope duplicate locals. It intentionally defers full type checking, interface satisfaction, flow checking, and expression typing. |
| HLSL emission | Reference includes emission path. | New AST HLSL M0 emitter exists under `Aurelian.Shaders.Language.Emission.Hlsl`; legacy emitter remains unchanged. | Compatible | A8 emits records/streams as structs, shader methods/stages as functions, and basic let/assignment/return/expression/empty/if/for statements plus basic expressions. Unsupported constructs produce diagnostics. No DXC/SPIR-V execution yet. |
| DXC/SPIR-V runner | Reference has runner/integration concepts. | Not implemented. | Deferred | Out of parser scope. |
| Artifact manifests | Reference has artifact concepts. | Legacy artifact path remains; new AST artifacts deferred. | Deferred | A8 emits HLSL text only; no new artifact manifest is produced yet. |
| Test modules/runner | Reference includes rich tests and prototype `.sdslvtest` support. | xUnit parser/AST/validation/emission tests in Aurelian, including the checked-in `tests/Aurelian.Shaders.Tests/Fixtures/Sdslv/smoke_triangle.sdslv` source fixture. | Intentionally different | A8 still intentionally uses ordinary xUnit tests only; `.sdslvtest` and CPU evaluator work remain deferred. |
| mixins | Not a native WyrmCoil SDSL-V production target for Aurelian. | Not native Aurelian SDSL-V. | Not applicable | Old Stride mixins are intentionally excluded. |
| Old Stride `.sdsl` compatibility | Historical reference only. | Not supported as native Aurelian SDSL-V. | Not applicable | No old Stride effect/base-shader inheritance model. |

## 4. Aurelian syntax decisions

- **Switch syntax:** Aurelian accepts `switch { case condition => value; else => value; }` and `switch subject { case value => result; else => result; }`. The `->` arrow is accepted as a compatibility parse shape, but `=>` is the preferred expression-arm spelling.
- **Match syntax:** Aurelian accepts `match subject { Path.Variant => value; else => value; }` plus fallible `ok(binding)` and `err(binding)` arms. The explicit `else` arm is an Aurelian parse-shape addition.
- **With syntax:** Aurelian prefers assignment-style updates: `base with { Field = value; Other = value; }`. Colon updates are also accepted for WyrmCoil-shaped compatibility.
- **Utility syntax:** Utility expressions are deferred in parser M1. The existing AST records preserve a future shape for ranked cases, guards, scores, and options.
- **Fallibility syntax:** Aurelian M1 documents prefix `try expression` and `unwrap expression`; postfix `expression?` and `expression!` are parsed as compatibility shapes.
- **Validation M0:** Aurelian A7 validates structural AST invariants and type-reference names only. It is not a full shader type checker and does not evaluate expression behavior.
- **HLSL emission M0:** Aurelian A8 emits deterministic HLSL from the new AST after callers explicitly parse and validate. It covers the smoke fixture shape and reports diagnostics for unsupported constructs rather than invoking DXC/SPIR-V or a renderer.

## 5. Known intentional divergences

- Aurelian does not support old Stride mixins as a native SDSL-V feature.
- Aurelian does not support old Stride effect/base-shader inheritance as a native SDSL-V model.
- Aurelian uses C# records/classes and result objects rather than Rust enum and parser result shapes.
- Aurelian may phase parser, validation, emission, artifact, and fixture work differently from WyrmCoil. A8 validates HLSL emission through ordinary xUnit tests and a checked-in `.sdslv` fixture rather than adding `.sdslvtest`.
- Aurelian may rename or reshape APIs where C# production needs differ, while preserving compatibility notes here.

## 6. Future `.sdslvtest` direction

Aurelian may support `.sdslvtest` files analogous to `.octest` by porting the Oct interpreter architecture to C# and using a CPU evaluator for deterministic shader behavior tests. This is intentionally deferred until parser/validation/emission contracts are stable. WyrmCoil remains reference-only for this direction and is not linked into production code.

## 7. Next compatibility work

- SDSL-V artifact manifest M0 for source/HLSL/diagnostic provenance after HLSL M0.
- Optional DXC validation M0 after artifact boundaries are explicit.
- Parser/validation M2 for flow, utility, and any remaining fallibility details.
- Artifact schema for new AST output.
- Shader fixtures that exercise accepted Aurelian syntax and documented WyrmCoil compatibility cases.
