# Cross-Project Patterns

## Sources reviewed

- `D:/FlashBackSDK/FlashBackSDK`
- `D:/FlashBackSDK/FBMinecraftMapClient`
- `D:/CS/ICS169/Capstone/ProjectCapstone`

## Recurring patterns worth preserving

### Composition root

Across projects, there is always a single place that owns startup order and manager creation:

- current project: `FBMainGame` plus `SystemConfig`
- `FBMinecraftMapClient`: same `FBMainGame` plus `SystemConfig` pattern
- `ProjectCapstone`: `PersistentGameManager`

This pattern is reusable product knowledge even though the specific implementation differs.

### Manager lifecycle

Across projects, long-lived systems share a predictable lifecycle:

- initialize once,
- survive scene changes,
- coordinate feature managers,
- handle global state changes.

In the current project and `FBMinecraftMapClient`, this is formalized by `FBGameSystem`.

### UI manager plus base view

Both newer and older projects use:

- a central UI manager,
- base UI types,
- config-driven or lookup-driven UI creation,
- persistence and visibility state management.

This is a high-value FlashBack pattern, but the current implementation still depends on project configs and generated metadata.

### Event bus with shared event catalog

Each project uses a centralized event catalog and manager-based dispatch. The reusable idea is the event bus itself; the current enum catalog remains project-specific.

### Resource abstraction

Each project introduces a resource-loading abstraction rather than directly scattering raw loads everywhere. The abstraction is reusable even when the exact Unity loading mechanism changes.

### Scene and level flow service

Scene transitions are consistently treated as a dedicated service, not ad hoc gameplay code.

## Repeated anti-patterns

### Global-singleton convenience becoming hard dependency

The convenience of global access (`FBMainGame.System`, singleton managers) helps prototypes ship quickly, but it makes later extraction and testing harder.

### Project contracts living inside nominally reusable modules

Examples:

- reusable services depending on `GameEvent.Event`
- UI runtime depending on project config catalogs and generation metadata
- network/runtime services assuming specific resource and handler naming

### Runtime/editor mixing

Performance and tooling code show how easy it is for runtime data models and editor authoring helpers to become entangled.

### Carried-over gameplay systems looking reusable

Battle, buff, or map systems may be inherited from older projects, but repeated reuse alone does not make them SDK-level. Reuse must be based on stable contracts, not just history.

## Strategic conclusions

1. Promote recurring patterns, not whole carried-over feature stacks.
2. Treat the composition root as a reusable architectural pattern but a project-specific implementation.
3. Convert repeated assumptions into documented contracts before calling a module product-ready.
4. Keep evidence from old projects because it shows what actually survives across projects.

