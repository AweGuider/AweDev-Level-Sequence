# Level Sequence Usage Guide

`AweDev Level Sequence` provides a small ScriptableObject workflow for defining levels, ordering them, validating scene setup, and navigating between scenes at runtime.

## Detailed Workflow

1. Create one `LevelDefinition` asset for each playable scene or menu scene.
2. Assign a stable level ID, display name, scene asset, and optional reference image.
3. Create a `LevelSequence` asset and add the level definitions in play order.
4. Add `LevelSequenceNavigatorBehaviour` to a scene or create `LevelSequenceNavigator` from code.
5. Sync Build Settings from the `LevelSequence` inspector.
6. Load levels through `SetLevel`, `SetLevelById`, or `MoveToNextLevel`.

## LevelDefinition Setup

Create a level definition from `Assets > Create > AweDev > Level Sequence > Level Definition`.

Recommended fields:

- `Level ID`: stable ID used by code and tools.
- `Display Name`: readable name for inspectors and UI.
- `Description`: optional authoring notes.
- `Scene Asset`: the Unity scene represented by this level.
- `Reference Image`: optional preview image for authoring.

When a scene asset is assigned in the editor, the package stores the scene name and scene path for runtime and validation use.

## LevelSequence Setup

Create a sequence from `Assets > Create > AweDev > Level Sequence > Level Sequence`.

Add each `LevelDefinition` to the sequence in navigation order. The first valid entry is the default build start level. Assign `Build Start Level` only when the build should start from a different level in the same sequence.

The `LevelSequence` inspector reports:

- duplicate level IDs
- null entries
- missing display names
- missing scene names or paths
- Build Settings drift

Click `Sync Build Settings` to add missing scenes, enable disabled sequence scenes, and move the effective build start scene to Build Settings index `0`. Build Settings are changed only when this button or the build prompt sync action is explicitly used.

## Runtime Navigation

Use `LevelSequenceNavigatorBehaviour` when you want scene-authored wiring:

```csharp
using AweDev.LevelSequence;
using UnityEngine;

public sealed class NextLevelTrigger : MonoBehaviour
{
    [SerializeField] private LevelSequenceNavigatorBehaviour _navigator;

    private void OnTriggerEnter(Collider other)
    {
        _navigator.MoveToNextLevel();
    }
}
```

Use `LevelSequenceNavigator` directly when the host project owns navigation from code or tests. The navigator can resolve levels by ID or scene name, track the current level, and publish events for load requests, loaded scenes, current-level changes, and navigation failures.

## Scene Loading And Custom Loaders

`UnitySceneLoader` is the default `ILevelSceneLoader`. It uses Unity scene management and requires scenes to be enabled in Build Settings.

Implement `ILevelSceneLoader` when the project needs custom behavior:

```csharp
using AweDev.LevelSequence;

public sealed class CustomLevelSceneLoader : ILevelSceneLoader
{
    public bool TryLoadLevel(LevelDefinition level, out string failureReason)
    {
        failureReason = null;
        // Route through addressables, fades, loading screens, save gates, etc.
        return true;
    }
}
```

Assign a custom loader with `SetSceneLoader` on `LevelSequenceNavigator` or `LevelSequenceNavigatorBehaviour`.

## Project Settings

`Project Settings > AweDev > Level Sequence` configures editor tooling, not runtime navigation.

Available settings:

- `Active Sequence`: sequence used by validation and build tooling.
- `Block invalid builds`: opt-in strict mode for build validation.
- `Prompt before build`: opt-in prompt for fixable Build Settings drift.
- `Reference Image Capture`: default output folder, filename format, and capture resolution.

Runtime navigation does not require an active sequence in Project Settings. Runtime uses the sequence assigned to your scene component or code-created navigator.

## Build Settings Sync And Reports

The `LevelSequence` inspector always reports Build Settings drift. It checks whether sequence scenes are missing, disabled, invalid, or whether the first Build Settings scene differs from the effective build start level.

`Sync Build Settings` can fix:

- missing sequence scenes
- disabled sequence scenes
- effective build start scene not being first

Invalid scene references are reported but skipped during sync.

## Build Prompt And Strict Blocking

Build tooling is opt-in:

- With `Prompt before build` enabled, interactive player builds can ask whether to sync fixable Build Settings drift.
- With `Block invalid builds` enabled, builds fail when the active sequence is missing, has duplicate level IDs, has invalid scene references, or has blocking Build Settings problems.
- In batch mode, strict blocking fails the build instead of showing dialogs.

Leaving both options off keeps validation visible in inspectors without changing build behavior.

## Reference Image Capture

The `LevelDefinition` inspector can capture a PNG from the active game camera. It prefers `Camera.main`, then the first enabled active scene camera.

Defaults:

- Output folder: `Assets/<UnityProjectName>/Level/Reference Images`
- Filename format: `{levelId}_reference`
- Resolution: `1024x576`

Filename tokens:

- `{levelId}`
- `{assetName}`
- `{displayName}`
- `{sceneName}`

Captures create the output folder when needed. Repeated captures overwrite the generated file for that level. If the current reference image points to a different manually assigned asset, the inspector asks before replacing it.

Capture is disabled when there is no active camera, the target `LevelDefinition` is not saved as a project asset, or the output folder is invalid.

## Sample Walkthrough

Import `Basic Level Flow` from Package Manager.

1. Open `LS_Sample_LevelSequence`.
2. Click `Sync Build Settings`.
3. Open `Sample Menu`.
4. Enter Play Mode.
5. Use menu buttons or colored trigger zones to load the next level, jump to a level, or return to the menu.

The sample contains only package-specific scripts and built-in Unity components.

## Troubleshooting

- Scene load fails: confirm the scene is enabled in Build Settings or use a custom `ILevelSceneLoader`.
- Build Settings report shows missing scenes: open the `LevelSequence` asset and click `Sync Build Settings`.
- Duplicate ID warning: give every `LevelDefinition` in the active sequence a unique level ID.
- Build prompt does not appear: enable `Prompt before build` in Project Settings.
- Build does not fail on validation warnings: enable `Block invalid builds` in Project Settings.
- Reference capture is disabled: save the `LevelDefinition` asset and make sure an enabled camera exists in the active scene.

## Known Limitations And Extension Points

- Default loading is scene-name based and Build Settings based.
- Addressables, additive loading, fades, save gates, and loading screens are extension points through `ILevelSceneLoader`.
- Project Settings are editor tooling settings; they are not runtime configuration.
- Reference image capture is editor-only.
