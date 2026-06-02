# Aurelian Architecture Charter

Aurelian is a greenfield C# engine/runtime created after the Stri-V rescue effort was paused. Stri-V remains valuable research, but Aurelian starts from a clean runtime foundation rather than treating Stride as the core engine base.

## Core principles

- Aurelian is a greenfield C# engine/runtime.
- Aurelian will adopt a Dominatus-native behavior/runtime spine beginning in A1, after Dominatus is vendored as buildable source under `vendor/Dominatus/`.
- The world model is explicit data first: engine state should be visible, testable, serializable where appropriate, and not hidden behind editor-first object graphs.
- Lifecycle flow should use typed lifecycle events rather than stringly or implicit processor callbacks.
- Side effects belong to actuators. Actuators own interactions with external systems and make effects intentional at architectural boundaries.
- Rendering should flow through render snapshots and command plans so runtime state and render submission remain separated.
- Asset work should move toward TOML/manifest-based assets.
- Shader work should move toward an SDSL-V-style compiler pipeline when that phase begins.
- Renderer/HAL selection is deferred until later MVP phases.

## Non-goals

- No Stride processor architecture as the runtime core.
- No Stride asset system as the Aurelian asset foundation.
- No editor-first strategy.
- No renderer implementation in A0.
- No window creation or triangle rendering in A0.
