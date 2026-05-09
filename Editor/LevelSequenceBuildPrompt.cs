using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AweDev.LevelSequence.Editor
{
    [InitializeOnLoad]
    public static class LevelSequenceBuildPrompt
    {
        static LevelSequenceBuildPrompt()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuildPlayer);
        }

        private static void OnBuildPlayer(BuildPlayerOptions options)
        {
            if (!LevelSequenceEditorSettings.promptBuildSync || Application.isBatchMode || TryPrepareBuild(ref options))
            {
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
        }

        private static bool TryPrepareBuild(ref BuildPlayerOptions options)
        {
            if (!LevelSequenceEditorSettings.TryGetActiveLevelSequence(out LevelSequence levelSequence, out string errorMessage))
            {
                return HandleInvalidBuildState(errorMessage);
            }

            List<DuplicateLevelIdGroup> duplicateGroups = LevelSequenceValidator.FindDuplicateLevelIds(levelSequence);
            if (duplicateGroups.Count > 0)
            {
                return HandleInvalidBuildState(
                    $"Duplicate level IDs found in active Level Sequence: {LevelSequenceValidator.FormatDuplicateIds(duplicateGroups)}.");
            }

            LevelSequenceBuildSettingsReport buildSettingsReport = LevelSequenceBuildSettingsSync.BuildReport(levelSequence);
            if (buildSettingsReport.invalidLevels.Count > 0)
            {
                return HandleInvalidBuildState(
                    $"Active Level Sequence contains invalid scene references:\n\n{LevelSequenceBuildSettingsSync.FormatInvalidLevels(buildSettingsReport)}");
            }

            if (!buildSettingsReport.hasFixableIssues) return true;

            LevelSequenceBuildSyncPromptResult promptResult = LevelSequenceBuildSyncPromptWindow.ShowPrompt(
                LevelSequenceBuildSettingsSync.FormatSummary(buildSettingsReport));

            switch (promptResult)
            {
                case LevelSequenceBuildSyncPromptResult.SyncAndContinue:
                    LevelSequenceBuildSettingsSyncResult result = LevelSequenceBuildSettingsSync.Sync(buildSettingsReport);
                    options.scenes = GetEnabledBuildScenePaths();
                    Debug.Log($"[LevelSequenceBuildPrompt] {LevelSequenceBuildSettingsSync.FormatSyncResult(result)}");
                    return true;
                case LevelSequenceBuildSyncPromptResult.ContinueWithoutSync:
                    Debug.LogWarning($"[LevelSequenceBuildPrompt] Continuing build without syncing Level Sequence Build Settings. {LevelSequenceBuildSettingsSync.FormatSummary(buildSettingsReport)}");
                    return true;
                default:
                    Debug.Log("[LevelSequenceBuildPrompt] Build cancelled from Level Sequence Build Settings prompt.");
                    return false;
            }
        }

        private static bool HandleInvalidBuildState(string message)
        {
            if (LevelSequenceEditorSettings.blockBuilds)
            {
                return ShowBuildBlocked(message);
            }

            Debug.LogWarning($"[LevelSequenceBuildPrompt] {message} Continuing because build blocking is disabled.");
            return true;
        }

        private static bool ShowBuildBlocked(string message)
        {
            string formattedMessage = $"[LevelSequenceBuildPrompt] {message}";
            Debug.LogError(formattedMessage);
            EditorUtility.DisplayDialog("Cannot Build Player", message, "OK");
            return false;
        }

        private static string[] GetEnabledBuildScenePaths()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }
    }

    internal enum LevelSequenceBuildSyncPromptResult
    {
        CancelBuild,
        SyncAndContinue,
        ContinueWithoutSync
    }

    internal sealed class LevelSequenceBuildSyncPromptWindow : EditorWindow
    {
        private const float WindowWidth = 460f;
        private const float WindowHeight = 220f;
        private const float ButtonHeight = 28f;

        private string _summary;
        private LevelSequenceBuildSyncPromptResult _result = LevelSequenceBuildSyncPromptResult.CancelBuild;

        public static LevelSequenceBuildSyncPromptResult ShowPrompt(string summary)
        {
            LevelSequenceBuildSyncPromptWindow window = CreateInstance<LevelSequenceBuildSyncPromptWindow>();
            window.titleContent = new GUIContent("Level Sequence Build Settings");
            window._summary = summary;
            window.minSize = new Vector2(WindowWidth, WindowHeight);
            window.maxSize = new Vector2(WindowWidth, WindowHeight);
            window.CenterOnMainWindow();
            window.ShowModalUtility();
            return window._result;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Build Settings Out Of Sync", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"The active Level Sequence is not synced with Build Settings.\n\n{_summary}\n\nChoose how to continue.",
                MessageType.Warning);

            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sync and Continue", GUILayout.Height(ButtonHeight)))
                {
                    _result = LevelSequenceBuildSyncPromptResult.SyncAndContinue;
                    Close();
                }

                if (GUILayout.Button("Continue Without Sync", GUILayout.Height(ButtonHeight)))
                {
                    _result = LevelSequenceBuildSyncPromptResult.ContinueWithoutSync;
                    Close();
                }
            }

            EditorGUILayout.Space(4f);

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                _result = LevelSequenceBuildSyncPromptResult.CancelBuild;
                Close();
                Event.current.Use();
            }
        }

        private void CenterOnMainWindow()
        {
            Rect mainWindowPosition = EditorGUIUtility.GetMainWindowPosition();
            position = new Rect(
                mainWindowPosition.x + (mainWindowPosition.width - WindowWidth) * 0.5f,
                mainWindowPosition.y + (mainWindowPosition.height - WindowHeight) * 0.5f,
                WindowWidth,
                WindowHeight);
        }
    }
}
