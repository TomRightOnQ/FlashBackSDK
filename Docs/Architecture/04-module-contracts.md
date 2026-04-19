# Module Contracts

## FlashBack

### `FBObject`

- Path: `Assets/Scripts/FlashBack/FBObject`
- Responsibility: object lifetime helpers and manager-level creation/destruction services.
- Public role: shared runtime foundation.
- Allowed dependencies: `FBGameSystem`, Unity runtime.
- Extraction blockers: lifecycle still depends on the current framework base class location.

### `EventSystem`

- Path: `Assets/Scripts/FlashBack/EventSystem`
- Responsibility: register listeners, dispatch events, and clean invalid listeners.
- Public role: cross-system communication bus.
- Allowed dependencies: neutral event contracts, Unity runtime.
- Current blockers:
  - event type comes from `GameEvent.Event`
  - event catalog is currently project-scoped

### `Time`

- Path: `Assets/Scripts/FlashBack/Time`
- Responsibility: timers and timer ownership per system.
- Public role: reusable async scheduling helper.
- Allowed dependencies: runtime base lifecycle only.
- Extraction blockers: none beyond current base-class placement.

### `ResourceSystem`

- Path: `Assets/Scripts/FlashBack/ResourceSystem`
- Responsibility: resource lookup and object loading.
- Public role: asset loading facade.
- Allowed dependencies: Unity asset-loading abstractions.
- Current blockers:
  - tied to Unity resource-loading strategy
  - not yet abstracted for engine-neutral use

### `UISystem`

- Path: `Assets/Scripts/FlashBack/UISystem`
- Responsibility: create, show, hide, remove, and query UI instances.
- Public role: shared UI runtime stack.
- Allowed dependencies: portable UI contracts and load services.
- Current blockers:
  - depends on `UIConfig`
  - depends on `FBUICreator`
  - depends on `FBMainGame.System`
  - assumes canvas layer discovery and current prefab conventions

### `Misc`

- Path: `Assets/Scripts/FlashBack/Misc`
- Responsibility: support utilities such as `ReadOnlyAttribute`.
- Public role: shared development convenience.
- Extraction blockers: pair editor drawer cleanup with runtime attribute ownership.

## Framework

### `GameLoop`

- Path: `Assets/Scripts/Framework/GameLoop`
- Responsibility: boot, host lifecycle, main-thread dispatch, and debug access.
- Public role: project composition host.
- Current blockers:
  - static global access
  - no namespace separation
  - partial-class composition

### `Contents`

- Path: `Assets/Scripts/Framework/Contents`
- Responsibility: project-facing config catalogs, enums, constants, and general data.
- Public role: shared data backbone for this game.
- Current blockers:
  - mixes generic and project-specific catalogs
  - currently owns event catalog and system composition declarations

### `LevelSystem`

- Path: `Assets/Scripts/Framework/LevelSystem`
- Responsibility: scene transition orchestration.
- Public role: shared project flow service.
- Current blockers:
  - directly uses `FBMainGame.System`
  - posts project event types

### `NetworkSystem`

- Path: `Assets/Scripts/Framework/NetworkSystem`
- Responsibility: transport, protocol loading, and handler dispatch.
- Public role: reusable networking candidate.
- Current blockers:
  - uses `Resources/NetworkProtocols`
  - reflection-driven handler lookup
  - requires `UnityMainThreadDispatcher`
  - assumes current message naming and handler naming

### `FBPerformanceDirector`

- Path: `Assets/Scripts/Framework/FBPerformanceDirector`
- Responsibility: performance asset model, timeline integration, and section/track definitions.
- Public role: reusable narrative/performance pipeline candidate.
- Current blockers:
  - runtime and editor concerns are mixed
  - terminology and scope are still evolving

### `Tools`

- Path: `Assets/Scripts/Framework/Tools`
- Responsibility: code generation and authoring pipelines.
- Public role: developer productivity tooling.
- Current blockers:
  - relies on current project path assumptions
  - runtime/editor separation needs hardening

## Gameplay

### `BattleSystem`

- Path: `Assets/Scripts/Gameplay/BattleSystem`
- Contract: concrete combat rules and combat-specific coordination.
- Tier: project-bound by default.

### `BuffSystem`

- Path: `Assets/Scripts/Gameplay/BuffSystem`
- Contract: status-effect logic and buff taxonomy.
- Tier: project-bound by default.

### `Components`

- Path: `Assets/Scripts/Gameplay/Components`
- Contract: unit-facing runtime state and gameplay data.
- Tier: project-bound by default.

### `Map`, `DialogueSystem`, `Template`, `NetHandlers`

- Path: `Assets/Scripts/Gameplay/Systems`, `Assets/Scripts/Gameplay/NetHandlers`
- Contract: feature-level gameplay and protocol glue.
- Tier: project-bound by default.

## Editor

### `Assets/Editor/FlashBack`

- Responsibility: editor assets and layout resources used by tools.
- Contract: tooling support assets only.
- Current blockers:
  - asset path stability during migration

### Editor folders under `Framework/Tools`

- Responsibility: editor windows, graph nodes, and code generation UI.
- Contract: authoring surfaces for framework/runtime features.
- Current blockers:
  - should be documented and packaged as editor-only features

