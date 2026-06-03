# Aurelian MVP Roadmap

- **A0 — Bootstrap:** create the clean solution, strict build discipline, architecture charter, project skeleton, and smoke tests without external runtime/vendor links.
- **A1 — Vendor Dominatus runtime smoke:** vendor Dominatus under `vendor/Dominatus/`, add buildable Dominatus projects to the solution, and add the first runtime smoke while keeping renderer work out of scope. **Completed in A1.**
- **A2 — Data world M0:** introduce minimal entity/world/component-store contracts, keep Dominatus linked but not over-integrated, produce a world snapshot/query surface, and keep rendering out of scope.
- **A3 — Actuation contracts:** define typed actuation boundaries and actuator-owned side-effect contracts.
- **A4 — Render snapshot:** define render snapshot contracts independent from a concrete backend.
- **A5 — Command plan:** introduce command-plan generation from snapshots.
- **A6 — Null renderer:** provide a non-windowed renderer implementation for deterministic tests.
- **A7 — First window/backend:** choose and integrate the first window/backend path.
- **A8 — First triangle:** render the first triangle through the established snapshot/command-plan/backend path.
