# Portability Matrix

## Rating guide

- `Portable`
  - Reusable with the same responsibilities and only data/config changes.
- `PortableWithAdapter`
  - Reusable, but extraction currently needs adapters, interfaces, renamed contracts, or asset path cleanup.
- `ProjectBound`
  - Tied to current game rules, content, or composition.

## Matrix

| Module | Current path | Responsibility | Tier | Main blockers |
| --- | --- | --- | --- | --- |
| FBObject | `Assets/Scripts/FlashBack/FBObject` | Shared object lifecycle and object manager services | Portable | Still inherits lifecycle from `FBGameSystem`, which currently lives in framework |
| EventSystem | `Assets/Scripts/FlashBack/EventSystem` | Generic event publish/subscribe and listener cleanup | PortableWithAdapter | Uses `GameEvent.Event`, which is project-bound today |
| Time | `Assets/Scripts/FlashBack/Time` | Timer services and timer ownership | Portable | Lifecycle base still depends on current `FBGameSystem` placement |
| ResourceSystem | `Assets/Scripts/FlashBack/ResourceSystem` | Resource loading facade | PortableWithAdapter | Unity `Resources` assumptions and current composition wiring |
| UISystem | `Assets/Scripts/FlashBack/UISystem` | UI lifetime, visibility, canvas layering, base UI types | PortableWithAdapter | Depends on `UIConfig`, `FBUICreator`, `FBMainGame.System`, and project naming conventions |
| Misc | `Assets/Scripts/FlashBack/Misc` | Shared attributes and support utilities | Portable | Editor drawer lives separately and should remain paired |
| GameLoop | `Assets/Scripts/Framework/GameLoop` | Startup host, lifecycle orchestration, debug, main-thread coordination | PortableWithAdapter | Static global access, partial composition, no namespace boundaries |
| Contents/Configs | `Assets/Scripts/Framework/Contents/Configs` | Shared catalogs, enums, prefab/UI config, composition declarations | ProjectBound | Mixture of reusable catalogs and project-specific contracts |
| Contents/General | `Assets/Scripts/Framework/Contents/General` | Shared data types and sample data | PortableWithAdapter | Needs separation between examples and real domain data |
| LevelSystem | `Assets/Scripts/Framework/LevelSystem` | Scene loading orchestration and level lifecycle | PortableWithAdapter | Depends on `FBMainGame.System` and project event catalog |
| NetworkSystem | `Assets/Scripts/Framework/NetworkSystem` | TCP transport, JSON protocol loading, handler dispatch | PortableWithAdapter | Resource path conventions, reflection lookup, dispatcher coupling, handler naming assumptions |
| FBPerformanceDirector | `Assets/Scripts/Framework/FBPerformanceDirector` | Performance/narrative asset model and track definitions | PortableWithAdapter | Runtime/editor boundary is mixed and dialogue assumptions may grow game-specific |
| Tools/FBUIPlugin | `Assets/Scripts/Framework/Tools/FBUIPlugin` | UI code generation and editor workflow helpers | PortableWithAdapter | Uses project path conventions and current folder assumptions |
| Tools/FBPerformanceDirector | `Assets/Scripts/Framework/Tools/FBPerformanceDirector` | Performance graph editor and tooling | PortableWithAdapter | Asset paths, editor/runtime coupling, and performance asset schema still in flux |
| Editor/FlashBack | `Assets/Editor/FlashBack` | Editor assets such as graph layout and styles | PortableWithAdapter | Asset path references are fragile during migration |
| BattleSystem | `Assets/Scripts/Gameplay/BattleSystem` | Battle orchestration and combat rules | ProjectBound | Concrete game rules |
| BuffSystem | `Assets/Scripts/Gameplay/BuffSystem` | Buff logic and buff tags | ProjectBound | Concrete game rules |
| Components | `Assets/Scripts/Gameplay/Components` | Units, attributes, factions, unit buff state | ProjectBound | Concrete game domain |
| Systems/Map | `Assets/Scripts/Gameplay/Systems/Map` | Map-specific UI and flow | ProjectBound | Concrete feature flow |
| Systems/DialogueSystem | `Assets/Scripts/Gameplay/Systems/DialogueSystem` | Dialogue-specific UI and feature logic | ProjectBound | Concrete feature flow |
| Systems/Template | `Assets/Scripts/Gameplay/Systems/Template` | Example/generated gameplay UI widgets | ProjectBound | Project-specific scaffolding |
| NetHandlers | `Assets/Scripts/Gameplay/NetHandlers` | Protocol-to-gameplay glue | ProjectBound | Depends on concrete gameplay systems |

## High-value extraction opportunities

### Strong candidates for future product capital

- `FBObject`
- `Time`
- `ResourceSystem`
- `EventSystem` after contract cleanup
- reusable parts of `UISystem`
- the `FBGameSystem` lifecycle pattern if detached from project-bound composition

### Reusable after adaptation

- `GameLoop`
- `LevelSystem`
- `NetworkSystem`
- `FBPerformanceDirector`
- editor toolchains

### Should remain project-specific

- battle, buffs, units, map flow, dialogue flow, gameplay protocol handlers
- config catalogs that encode current project semantics
- `SystemConfig` because it composes a concrete game

## Cross-project patterns

The following patterns already recur across this project and older projects:

- central composition root
- manager lifecycle initialization order
- UI manager plus base view types
- event bus with a shared event catalog
- resource manager abstraction
- scene/level flow orchestration
- config-driven setup

Those patterns are stronger product candidates than any single concrete gameplay system.

## Extraction backlog

1. Move cross-project contracts out of `Framework/Contents/Configs` when they are not project-specific.
2. Reduce direct `FBMainGame.System` coupling inside modules that aim to become portable.
3. Separate editor-only and runtime-only performance tooling.
4. Replace folder and asset path assumptions in UI/plugin tooling with configurable paths.
5. Introduce namespaces that reflect the portability layers.

