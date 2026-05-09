using UnityEditor;
using UnityEngine;

namespace AweDev.LevelSequence.Editor
{
    public static class LevelSequenceSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/AweDev/Level Sequence", SettingsScope.Project)
            {
                label = "Level Sequence",
                guiHandler = _ =>
                {
                    EditorGUILayout.LabelField("Active Sequence", EditorStyles.boldLabel);

                    LevelSequence activeSequence = LevelSequenceEditorSettings.activeSequence;
                    LevelSequence nextSequence = (LevelSequence)EditorGUILayout.ObjectField(
                        "Level Sequence",
                        activeSequence,
                        typeof(LevelSequence),
                        false);

                    if (nextSequence != activeSequence)
                    {
                        LevelSequenceEditorSettings.activeSequence = nextSequence;
                    }

                    EditorGUILayout.Space(8f);
                    EditorGUILayout.LabelField("Build Validation", EditorStyles.boldLabel);
                    LevelSequenceEditorSettings.blockBuilds = EditorGUILayout.ToggleLeft(
                        new GUIContent(
                            "Block invalid builds",
                            "Strict mode. Blocks player builds when the active Level Sequence has duplicate IDs, invalid scene references, or other blocking validation errors."),
                        LevelSequenceEditorSettings.blockBuilds);
                    LevelSequenceEditorSettings.promptBuildSync = EditorGUILayout.ToggleLeft(
                        new GUIContent(
                            "Prompt before build",
                            "Before a player build, ask whether to sync fixable Build Settings drift for the active Level Sequence."),
                        LevelSequenceEditorSettings.promptBuildSync);

                    EditorGUILayout.HelpBox(
                        "Inspector reports are always available. Build blocking and sync prompts are opt-in. Runtime navigation uses the Level Sequence assigned to your scene components or code.",
                        MessageType.Info);

                    EditorGUILayout.Space(8f);
                    EditorGUILayout.LabelField("Reference Image Capture", EditorStyles.boldLabel);

                    LevelSequenceEditorSettings.useProductNameForDefaultCaptureFolder = EditorGUILayout.Toggle(
                        new GUIContent(
                            "Use Product Name",
                            "Use Project Settings > Player > Product Name instead of the Unity project folder name for the default capture folder."),
                        LevelSequenceEditorSettings.useProductNameForDefaultCaptureFolder);
                    LevelSequenceEditorSettings.referenceImageOutputFolder = EditorGUILayout.TextField(
                        new GUIContent(
                            "Output Folder",
                            "Folder under Assets where captured reference images are saved. Leave this at the computed default to use the Unity project folder name, or Product Name when enabled."),
                        LevelSequenceEditorSettings.referenceImageOutputFolder);
                    LevelSequenceEditorSettings.referenceImageFilenameFormat = EditorGUILayout.TextField(
                        new GUIContent(
                            "Filename Format",
                            "Supported tokens: Level ID {levelId}, asset name {assetName}, display name {displayName}, and scene name {sceneName}. Example: {levelId}_reference."),
                        LevelSequenceEditorSettings.referenceImageFilenameFormat);
                    LevelSequenceEditorSettings.referenceImageCaptureWidth = EditorGUILayout.IntField(
                        "Capture Width",
                        LevelSequenceEditorSettings.referenceImageCaptureWidth);
                    LevelSequenceEditorSettings.referenceImageCaptureHeight = EditorGUILayout.IntField(
                        "Capture Height",
                        LevelSequenceEditorSettings.referenceImageCaptureHeight);

                    EditorGUILayout.HelpBox(
                        "Reference captures are saved as PNG assets. The default folder uses the Unity project folder name unless Product Name is enabled. Repeated captures overwrite the generated file for the level.",
                        MessageType.Info);
                },
                keywords = new[] { "level", "sequence", "build", "validation", "reference", "capture" }
            };
        }
    }
}
