# ADR-0001: Layer Model

## Status

Accepted

## Context

FlashBack is growing from a graduation-project framework and repeated jam-era reuse into something that may become long-term technical capital. The codebase already mixes:

- reusable core systems,
- project-specific framework glue,
- gameplay rules and content,
- editor tooling.

Without a clear layer model, modules drift between "SDK" and "game code" based only on convenience.

## Decision

Adopt the following architectural layers:

- `FlashBack`
  - reusable modules intended to survive project swaps
- `Framework`
  - project-specific composition and shared infrastructure
- `Gameplay`
  - rules, content logic, and feature-specific behavior
- `Editor/FlashBack`
  - editor tooling for reusable and framework systems

Portability is now an explicit classification decision:

- `Portable`
- `PortableWithAdapter`
- `ProjectBound`

## Consequences

### Positive

- reusable product capital becomes easier to identify,
- documentation can describe stable boundaries,
- future extraction into packages or another engine becomes more deliberate,
- project-specific glue remains allowed without pretending it is SDK-ready.

### Negative

- more documentation is required,
- some existing "SDK-like" modules will be reclassified downward,
- current folder names and code dependencies do not fully match the new model yet.

## Follow-up

1. Maintain the portability matrix.
2. Document the composition root.
3. Refresh workspace rules to follow the new model.
4. Continue physical migration only after documentation and dependency cleanup.

