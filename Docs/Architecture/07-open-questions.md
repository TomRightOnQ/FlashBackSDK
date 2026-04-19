# Open Questions and TODOs

## Questions for future review

### Product boundary

- Should `FBGameSystem` move into `FlashBack` as the canonical reusable lifecycle base, or should FlashBack define a cleaner next-generation base type?
- Should the event catalog remain enum-based, or should a future version support typed event identifiers that are not bound to one project's `GameEvent` file?
- Which parts of `UISystem` should stay runtime-only and which parts should become generator/tooling contracts?

### Runtime architecture

- Should `FBMainGame.System` remain the primary access pattern, or should long-term productization move toward explicit registration and lookup contracts?
- Should `SystemConfig` stay as a partial of `FBMainGame`, or should the composition list eventually move to a separate installer-style contract?
- Is `FBLevelSystem` only a thin Unity scene helper, or should it become a broader application-state flow service?

### Tooling

- Should `FBUIPlugin` target configurable output paths instead of assuming current project structure?
- Should performance tooling be split into:
  - runtime data model,
  - runtime playback,
  - editor authoring UI?
- Which editor utilities belong in `Assets/Editor/FlashBack` versus editor folders near the owning runtime modules?

### Naming and packaging

- Should remaining legacy API names such as `Perfmance*` be cleaned before package extraction?
- When namespaces are introduced, should they mirror physical folders exactly?
- At what point does FlashBack become a package with asmdefs instead of a project-local layer?

## Advice backlog

- Document every new reusable module before promoting it into `FlashBack`.
- Keep feature-specific enums and rule catalogs out of `FlashBack`.
- Whenever a module requires `FBMainGame.System`, write down whether that dependency is essential or only convenient.
- Use old projects as evidence for recurring patterns, not as proof that every carried-over class belongs in the SDK.
- When a gameplay system feels reusable, separate the abstract rule from the current content implementation before moving it.

