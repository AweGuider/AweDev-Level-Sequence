# Basic Level Flow Sample

This sample is intentionally small. Create a `LevelSequenceNavigatorBehaviour`, assign a `LevelSequence`, and use `AweDev.LevelSequence.Samples.LevelSequenceNextLevelTrigger` or UI buttons to call `MoveToNextLevel`, `SetLevel`, or `SetLevelById`.

The sample uses only built-in Unity components for labels and controls. TextMesh Pro is not required.

For custom loading flows, call `SetSceneLoader` on the navigator and provide an `ILevelSceneLoader` implementation.
