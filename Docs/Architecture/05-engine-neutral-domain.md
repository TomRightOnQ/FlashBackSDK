# Engine-Neutral Domain Notes

## Why this document exists

If FlashBack ever moves away from Unity, the most valuable things to preserve are not `MonoBehaviour` wrappers. The valuable parts are:

- the composition model,
- the system lifecycle model,
- data contracts,
- event semantics,
- UI/view state semantics,
- narrative/performance graph concepts,
- and gameplay rules.

This document records those engine-neutral ideas.

## Concepts that should survive an engine change

### Composition root

A single host should still:

- create system instances,
- define startup order,
- manage shutdown,
- coordinate scene or state transitions,
- expose a clear place where a game is composed from reusable modules.

The host does not need to remain `MonoBehaviour`-based, but the concept should remain.

### System lifecycle

`FBGameSystem` implies a lifecycle model that is engine-neutral:

- create
- init
- pre-scene-unload
- on-scene-change
- post-scene-load-complete
- manual init hook

These phases are portable even if their Unity implementation changes.

### Event bus

The event bus concept is portable:

- subscribe,
- unsubscribe,
- dispatch payload-bearing events,
- clean dead listeners.

The current `GameEvent.Event` enum is not engine-neutral; the concept of event contracts is.

### UI state model

The following UI concepts are portable:

- create view instance
- show / hide view
- remove view
- persistence across scene changes
- auto-show behavior
- layered presentation

The current use of `Resources`, `FBUICreator`, and Unity canvases is implementation-specific.

### Narrative/performance model

The performance system already contains engine-neutral ideas:

- a scene graph of sections,
- entry and exit nodes,
- typed output events,
- graph connections,
- section-local assets,
- custom variables carried by transitions.

The current Timeline and editor implementation is Unity-specific; the content graph is not.

### Gameplay rules

Battle, buffs, unit stats, faction relations, and dialogue flow may be project-bound, but their underlying rules should still be described in engine-neutral terms when they become stable enough.

## What is Unity-specific today

- `MonoBehaviour` inheritance
- `DontDestroyOnLoad`
- `Resources.Load`
- `SceneManager.LoadSceneAsync`
- `ScriptableObject` authoring
- `Timeline` integration
- editor windows, drawers, graph views, and asset database usage

## Documentation rule

Whenever a module is important enough to keep during an engine migration, document:

1. the engine-neutral concept,
2. the Unity-specific implementation,
3. the migration-sensitive assumptions,
4. the data shape that should remain stable.

## Translation template

Use this template for future docs:

- Concept
- Why it exists
- Stable data/behavior contract
- Current Unity implementation
- Portability risks
- Engine migration notes

