# AweDev Level Sequence

Reusable level definition, ordered sequence navigation, and editor validation tools for Unity projects.

Use this package when a project needs a small ScriptableObject-based level list, predictable next-level navigation, Build Settings checks, and replaceable scene loading without bringing in a larger progression framework.

## Key Features

- `LevelDefinition` assets for stable level IDs, display names, scene references, and optional reference images.
- `LevelSequence` assets for ordering levels and choosing an optional build start level.
- `LevelSequenceNavigator` and `LevelSequenceNavigatorBehaviour` for runtime navigation.
- Replaceable scene loading through `ILevelSceneLoader`.
- Inspector validation for duplicate IDs, null entries, missing scene paths, and Build Settings drift.
- Explicit Build Settings sync from the `LevelSequence` inspector.
- Optional build prompt and strict build blocking through Project Settings.
- Reference image capture from the active game camera.
- Minimal `Basic Level Flow` sample under `Samples~`.

## Installation

Install from Git URL in Unity Package Manager:

1. Open `Window > Package Manager`.
2. Click `+` and choose `Add package from git URL...`.
3. Enter:

```text
https://github.com/AweGuider/com.awedev.level-sequence.git
```

For a tagged release, append the tag:

```text
https://github.com/AweGuider/com.awedev.level-sequence.git#v0.1.2
```

## Quick Start

1. Create one `LevelDefinition` asset per scene from `Assets > Create > AweDev > Level Sequence > Level Definition`.
2. Assign a stable level ID, display name, and scene asset on each `LevelDefinition`.
3. Create a `LevelSequence` asset and order the levels.
4. Add `LevelSequenceNavigatorBehaviour` to a scene and assign the `LevelSequence`.
5. Open the `LevelSequence` asset and click `Sync Build Settings`.
6. Call `MoveToNextLevel`, `SetLevel`, or `SetLevelById` from UI, triggers, or code.

## Sample

Import `Basic Level Flow` from Package Manager to get a small menu scene, two level scenes, trigger scripts, and sample `LevelDefinition` / `LevelSequence` assets.

After importing the sample:

1. Open `LS_Sample_LevelSequence`.
2. Click `Sync Build Settings` in the inspector.
3. Open `Sample Menu`.
4. Enter Play Mode and use the buttons or colored trigger zones to move through the sample flow.

The sample uses built-in Unity text components and does not require TextMesh Pro.

## Basic Runtime Usage

`UnitySceneLoader` is the default loader. It loads by scene name, so the target scenes must be enabled in Build Settings.

```csharp
using AweDev.LevelSequence;
using UnityEngine;

public sealed class NextLevelButton : MonoBehaviour
{
    [SerializeField] private LevelSequenceNavigatorBehaviour _navigator;

    public void LoadNextLevel()
    {
        _navigator.MoveToNextLevel();
    }
}
```

Host projects can replace scene loading by implementing `ILevelSceneLoader` for addressables, loading screens, fades, additive scenes, save gates, or tests.

## Detailed Documentation

See [Documentation~/level-sequence.md](Documentation~/level-sequence.md) for the full workflow, Project Settings, Build Settings validation, reference image capture, custom scene loaders, troubleshooting, and extension points.

## Known Limitations

- `UnitySceneLoader` requires level scenes to be enabled in Build Settings.
- Build Settings sync mutates Build Settings only when explicitly invoked.
- Build blocking and build-sync prompts are opt-in editor tools.
- Project Settings configure editor tooling; runtime navigation uses the `LevelSequence` assigned in scene components or code.
- Reference image capture is editor-only and writes PNG files under the host project's `Assets` folder.
- Addressables and custom transition flows require a custom `ILevelSceneLoader`.

## License

MIT. See [LICENSE.md](LICENSE.md).
