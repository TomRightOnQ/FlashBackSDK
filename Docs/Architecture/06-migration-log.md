# Migration Log

## Purpose

Record architectural moves, naming shifts, and portability-driven cleanup so the project history remains understandable even after multiple refactors.

## Current migration state

### Documentation

- Added a tracked documentation root under `Docs/Architecture` and `Docs/ADR`.
- `Markdowns/` remains a private scratch space because it is ignored by `.gitignore`.
- The durable handbook now lives under `Docs`.

### Layer model adopted

- `FlashBack` = reusable product-capital candidates
- `Framework` = project composition and shared infrastructure
- `Gameplay` = rules and feature logic
- `Editor/FlashBack` = editor tooling surface

### Transitional path note

The architecture language has moved faster than the physical folders. Some paths and prior notes still reference legacy names such as:

- `FrameWork`
- `GamePlay`
- top-level `Contents`
- top-level `Tools`

The root folder migration is now in place. Keep the legacy names only as historical references when reading older notes, old projects, or pre-migration scripts.

## Migration principles

1. Document before moving code.
2. Classify before extracting.
3. Preserve history of why a folder or module changed layers.
4. Avoid moving a module into `FlashBack` unless its dependencies and contract are documented.
5. Prefer multiple small migrations over one broad relocation.

## Planned migration sequence

1. docs and rules
2. namespace and naming cleanup
3. editor/runtime boundary cleanup
4. dependency cleanup per module
5. structural folder migration
6. optional asmdefs and package extraction later

## Log entries

### 2026-04-19

- Created the tracked architecture handbook.
- Established the three-layer product model as the current architectural standard.
- Marked `FBMainGame` plus `SystemConfig` as the composition root.
- Added a portability matrix and module contracts.
- Added open questions and future commercialization notes.
- Normalized the main script roots to `FlashBack`, `Framework`, `Gameplay`, and `Assets/Editor/FlashBack`.
- Removed empty legacy roots left behind by the earlier migration pass.

