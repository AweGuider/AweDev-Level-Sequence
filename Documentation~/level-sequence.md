# Level Sequence

`LevelSequence` is an ordered list of `LevelDefinition` assets. `LevelSequenceNavigator` owns navigation through that list.

## Core Workflow

1. Create one `LevelDefinition` asset per level.
2. Assign a stable level ID, display name, scene asset, and optional reference image.
3. Create a `LevelSequence` asset and order the levels.
4. Add `LevelSequenceNavigatorBehaviour` to a scene or create `LevelSequenceNavigator` from code.
5. Call `SetLevel`, `SetLevelById`, or `MoveToNextLevel`.

Scene loading is replaceable:

- `UnitySceneLoader` is the default loader.
- Implement `ILevelSceneLoader` to route transitions through addressables, loading screens, additive scenes, save checks, or custom fades.

## Project Settings

`Project Settings > AweDev > Level Sequence` configures editor tooling, not runtime navigation.

- Active Sequence: used by inspectors and validation tools.
- Block invalid builds: strict opt-in build validation.
- Prompt before build: asks whether to sync fixable Build Settings drift before a player build.
- Reference Image Capture: shared defaults for output folder, filename format, and capture resolution.

Inspector reports are always available. Build blocking and build prompts are opt-in.

## Reference Image Capture

The `LevelDefinition` inspector can capture a PNG from the active game camera. It prefers `Camera.main`, then the first enabled active scene camera.

Defaults:

- Output folder: `Assets/<UnityProjectName>/Level/Reference Images`
- Filename format: `{levelId}_reference`
- Resolution: `1024x576`

Project Settings can switch the default output folder source from Unity project name to Product Name. A manually edited output folder overrides either default.

Filename tokens:

- Level ID: `{levelId}`
- asset name: `{assetName}`
- display name: `{displayName}`
- scene name: `{sceneName}`

Captures create the output folder when needed. Repeated captures overwrite the generated file for the level. If the current reference image points to a different manually assigned asset, the inspector asks before replacing it.

Capture is disabled when there is no active camera, the target `LevelDefinition` is not saved as a project asset, or the output folder is invalid.
