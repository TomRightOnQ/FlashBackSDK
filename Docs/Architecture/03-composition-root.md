# Composition Root

## Definition

The composition root is the place where reusable systems become a concrete game. In this project, that root is formed by:

- `Assets/Scripts/Framework/GameLoop/FBMainGame.cs`
- `Assets/Scripts/Framework/Contents/Configs/SystemConfig.cs`

`FBMainGame` owns application lifetime. `SystemConfig` decides which systems exist in this game and in what order they are created.

## Why this matters

If a module can only run when `FBMainGame` and `SystemConfig` have already wired it into the world, that module is not yet a standalone SDK product. It may still be reusable, but its integration contract is implicit rather than formalized.

## Core pattern

`FBMainGame` acts as a globally reachable host:

```6:16:Assets/Scripts/Framework/GameLoop/FBMainGame.cs
/// <summary>
/// THIS IS THE CORE BEGINNING/ENTRY POINT OF THE GAME
/// FBMain Game will be placed in the game's first loading scene,
/// holding the references of all systems, and persists through the game
/// </summary>
public partial class FBMainGame : MonoBehaviour
{
    private static FBMainGame instance;
    public static FBMainGame System => instance;
```

`SystemConfig` composes the concrete system graph:

```35:47:Assets/Scripts/Framework/Contents/Configs/SystemConfig.cs
    private void CreateSystem()
    {
        // 1. FBResourceManager
        GameObject O_FBResourceManager = new GameObject("FBResourceManager");
        FBResourceManager = O_FBResourceManager.AddComponent<FBResourceManager>();
        gameSystemDictionary["FBResourceManager"] = FBResourceManager;
        FBResourceManager.OnSystemCreate();
```

## Responsibilities

### `FBMainGame`

- boot the application once,
- survive scene changes,
- own the shared system dictionary,
- call the system lifecycle phases,
- expose global access through `FBMainGame.System`.

### `SystemConfig`

- declare the system fields that the game can access,
- instantiate concrete systems,
- register them in the creation dictionary,
- encode startup order.

## Rule of interpretation

- `FBMainGame` is not the SDK itself.
- `SystemConfig` is not reusable product logic; it is game composition.
- A reusable module may be created by `SystemConfig`, but that does not make the composition root reusable.

## Current strengths

- very clear startup location,
- simple mental model for small projects and jams,
- easy global access from gameplay code,
- fast to bootstrap new experiments.

## Current portability costs

- strong global singleton coupling,
- implicit service locator through `FBMainGame.System`,
- partial-class composition binds system declarations and game host together,
- system contracts are not explicit interfaces yet,
- reusable modules can accidentally depend on project-level events and configs.

## Target evolution

Keep the composition-root concept, but make it cleaner:

1. Preserve a single host that wires systems together.
2. Push reusable modules toward explicit contracts and documented dependencies.
3. Treat project composition as a separate concern from SDK capabilities.
4. Over time, allow different hosts to compose FlashBack modules without changing the modules themselves.

## Immediate guidance

When adding a new system:

- put the system implementation in `FlashBack` only if it can survive another project's boot process,
- put its registration in `SystemConfig` only if the current game should own the composition,
- document the dependency in the portability matrix before calling it reusable.

