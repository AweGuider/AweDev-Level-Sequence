# AweDev Level Sequence

Reusable level definition, ordered sequence navigation, and editor validation tools for Unity.

## Install

Install from Git in Unity Package Manager:

1. Open `Window > Package Manager`.
2. Click `+` and choose `Add package from git URL...`.
3. Enter the package repository URL.
4. Select `AweDev Level Sequence` after Unity resolves the package.

## Quick Start

1. Create one `LevelDefinition` asset per scene from `Assets > Create > AweDev > Level Sequence > Level Definition`.
2. Assign a stable level ID, display name, and scene asset on each `LevelDefinition`.
3. Create a `LevelSequence` asset and order the levels.
4. Add `LevelSequenceNavigatorBehaviour` to a scene and assign the `LevelSequence`.
5. Open the `LevelSequence` asset and click `Sync Build Settings`.
6. Call `MoveToNextLevel`, `SetLevel`, or `SetLevelById` from UI, triggers, or code.

## Runtime

Create `LevelDefinition` assets for each level, assign their scene assets, then add them to a `LevelSequence`. Use `LevelSequenceNavigator` directly from code or `LevelSequenceNavigatorBehaviour` in a scene to resolve levels, move to the next level, and request scene loads.

`UnitySceneLoader` is the default loader and uses Unity scene management. Host projects can replace it with an `ILevelSceneLoader` implementation for addressables, additive loading, fades, loading screens, save gates, or tests.

## Editor Tools

The `LevelSequence` inspector reports duplicate IDs, null entries, scene path issues, and Build Settings drift. The sync button updates Build Settings only when clicked.

`Project Settings > AweDev > Level Sequence` configures editor-only tooling:

- active sequence for validation
- optional build blocking
- optional Build Settings sync prompt
- reference image capture folder, filename format, and resolution

Runtime navigation does not depend on these Project Settings.

## Reference Images

The `LevelDefinition` inspector can capture a PNG reference image from the active game camera. The default output folder is `Assets/<UnityProjectName>/Level/Reference Images`, and the default filename format is `{levelId}_reference`. Project Settings can switch the default folder source from Unity project name to Product Name.

Repeated captures overwrite the generated file for that level. If the level currently references a different manually assigned texture, the inspector asks for confirmation before replacing the reference.

Filename format tokens include Level ID `{levelId}`, asset name `{assetName}`, display name `{displayName}`, and scene name `{sceneName}`.

## Samples

Import `Basic Level Flow` from Package Manager to get a minimal menu, level scenes, trigger scripts, and sample `LevelDefinition` / `LevelSequence` assets.

After importing the sample:

1. Open `LS_Sample_LevelSequence`.
2. Click `Sync Build Settings` in the inspector.
3. Open `Sample Menu`.
4. Enter Play Mode and use the buttons or colored trigger zones to move through the sample flow.

The sample uses built-in Unity text components and does not require TextMesh Pro.

## Known Limitations

- The default `UnitySceneLoader` loads scenes by scene name and requires those scenes to be enabled in Build Settings.
- Build blocking and build-sync prompts are opt-in editor tools.
- Reference image capture is editor-only and writes PNG files under the host project's `Assets` folder.
- Addressables, loading screens, fades, save gates, and additive loading should be implemented through a custom `ILevelSceneLoader`.
