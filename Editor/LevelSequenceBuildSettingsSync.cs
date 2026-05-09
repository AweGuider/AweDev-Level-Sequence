using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AweDev.LevelSequence.Editor
{
    public static class LevelSequenceBuildSettingsSync
    {
        public static LevelSequenceBuildSettingsReport BuildReport(LevelSequence levelSequence)
        {
            LevelSequenceBuildSettingsReport report = new();
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            Dictionary<string, EditorBuildSettingsScene> buildScenesByPath = new();
            HashSet<string> sequenceScenePaths = new();
            string firstBuildScenePath = buildScenes.Length > 0 ? buildScenes[0].path : null;

            foreach (EditorBuildSettingsScene buildScene in buildScenes)
            {
                if (string.IsNullOrWhiteSpace(buildScene.path) || buildScenesByPath.ContainsKey(buildScene.path)) continue;

                buildScenesByPath.Add(buildScene.path, buildScene);
            }

            if (levelSequence == null)
            {
                report.invalidLevels.Add(new LevelSequenceBuildSettingsInvalidLevel(-1, null, "Level Sequence is missing."));
                return report;
            }

            LevelDefinition effectiveStartLevel = levelSequence.GetEffectiveBuildStartLevel();
            report.effectiveBuildStartLevel = effectiveStartLevel;
            report.usesBuildStartOverride = levelSequence.buildStartLevel != null;

            if (effectiveStartLevel == null)
            {
                report.invalidLevels.Add(new LevelSequenceBuildSettingsInvalidLevel(-1, null, "Level Sequence has no effective build start level."));
            }
            else if (!ContainsLevel(levelSequence, effectiveStartLevel))
            {
                report.invalidLevels.Add(new LevelSequenceBuildSettingsInvalidLevel(-1, effectiveStartLevel, $"Build start level '{effectiveStartLevel.name}' is not included in the Level Sequence."));
            }
            else if (TryGetLevelScenePath(effectiveStartLevel, out string startScenePath, out string startInvalidReason))
            {
                report.effectiveBuildStartScenePath = startScenePath;

                if (firstBuildScenePath != startScenePath)
                {
                    report.isFirstSceneOutOfSync = true;
                }
            }
            else
            {
                // Sequence entries are validated below. Avoid reporting the effective start level twice.
            }

            for (int i = 0; i < levelSequence.levels.Count; i++)
            {
                LevelDefinition level = levelSequence.levels[i];

                if (!TryGetLevelScenePath(level, out string scenePath, out string invalidReason))
                {
                    report.invalidLevels.Add(new LevelSequenceBuildSettingsInvalidLevel(i, level, invalidReason));
                    continue;
                }

                if (!sequenceScenePaths.Add(scenePath)) continue;

                if (!buildScenesByPath.TryGetValue(scenePath, out EditorBuildSettingsScene buildScene))
                {
                    report.missingScenes.Add(scenePath);
                    continue;
                }

                if (!buildScene.enabled)
                {
                    report.disabledScenes.Add(scenePath);
                }
            }

            return report;
        }

        public static LevelSequenceBuildSettingsSyncResult Sync(LevelSequenceBuildSettingsReport report)
        {
            if (report == null) return new LevelSequenceBuildSettingsSyncResult(0, 0, 0, 0);

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            List<EditorBuildSettingsScene> updatedScenes = new();
            HashSet<string> disabledScenes = new(report.disabledScenes);
            HashSet<string> existingScenePaths = new();
            int enabledCount = 0;
            int addedCount = 0;
            int movedStartSceneCount = 0;
            string startScenePath = report.effectiveBuildStartScenePath;

            if (!string.IsNullOrWhiteSpace(startScenePath))
            {
                bool startSceneExists = false;
                bool startSceneWasFirst = false;

                for (int i = 0; i < buildScenes.Length; i++)
                {
                    EditorBuildSettingsScene buildScene = buildScenes[i];
                    if (buildScene.path != startScenePath) continue;

                    startSceneExists = true;
                    startSceneWasFirst = i == 0 && buildScene.enabled;

                    if (!buildScene.enabled) enabledCount++;

                    break;
                }

                if (!startSceneExists) addedCount++;
                if (!startSceneWasFirst) movedStartSceneCount++;

                updatedScenes.Add(new EditorBuildSettingsScene(startScenePath, true));
                existingScenePaths.Add(startScenePath);
            }

            foreach (EditorBuildSettingsScene buildScene in buildScenes)
            {
                if (string.IsNullOrWhiteSpace(buildScene.path) || !existingScenePaths.Add(buildScene.path)) continue;

                if (disabledScenes.Contains(buildScene.path))
                {
                    updatedScenes.Add(new EditorBuildSettingsScene(buildScene.path, true));
                    enabledCount++;
                    continue;
                }

                updatedScenes.Add(buildScene);
            }

            foreach (string scenePath in report.missingScenes)
            {
                if (!existingScenePaths.Add(scenePath)) continue;

                updatedScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                addedCount++;
            }

            EditorBuildSettings.scenes = updatedScenes.ToArray();
            return new LevelSequenceBuildSettingsSyncResult(addedCount, enabledCount, report.invalidLevels.Count, movedStartSceneCount);
        }

        public static string FormatSummary(LevelSequenceBuildSettingsReport report)
        {
            if (report == null) return "Build Settings report could not be created.";

            List<string> warnings = new();
            if (report.isFirstSceneOutOfSync) warnings.Add("Build Settings index 0 does not match the effective build start level");
            if (report.missingScenes.Count > 0) warnings.Add($"{report.missingScenes.Count} missing from Build Settings");
            if (report.disabledScenes.Count > 0) warnings.Add($"{report.disabledScenes.Count} disabled in Build Settings");
            if (report.invalidLevels.Count > 0) warnings.Add($"{report.invalidLevels.Count} invalid or missing scene paths");

            return warnings.Count > 0 ? string.Join(", ", warnings) : "All Level Sequence scenes are enabled in Build Settings.";
        }

        public static string FormatSyncResult(LevelSequenceBuildSettingsSyncResult result)
        {
            return $"Build Settings sync complete. Added: {result.addedCount}, enabled: {result.enabledCount}, moved start scene: {result.movedStartSceneCount}, skipped invalid levels: {result.skippedInvalidLevelCount}.";
        }

        public static string FormatInvalidLevels(LevelSequenceBuildSettingsReport report, int maxItems = 5)
        {
            if (report == null || report.invalidLevels.Count == 0) return string.Empty;

            List<string> invalidLevels = new();
            int count = Mathf.Min(report.invalidLevels.Count, maxItems);

            for (int i = 0; i < count; i++)
            {
                invalidLevels.Add(report.invalidLevels[i].ToString());
            }

            if (report.invalidLevels.Count > maxItems)
            {
                invalidLevels.Add($"...and {report.invalidLevels.Count - maxItems} more.");
            }

            return string.Join("\n", invalidLevels);
        }

        private static bool TryGetLevelScenePath(LevelDefinition level, out string scenePath, out string invalidReason)
        {
            scenePath = null;
            invalidReason = null;

            if (level == null)
            {
                invalidReason = "Level entry is null.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(level.scenePath))
            {
                scenePath = level.scenePath;
            }
            else if (level.sceneAsset != null)
            {
                scenePath = AssetDatabase.GetAssetPath(level.sceneAsset);
            }

            if (string.IsNullOrWhiteSpace(scenePath))
            {
                invalidReason = $"Level '{level.name}' has no scene path.";
                return false;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null) return true;

            invalidReason = $"Level '{level.name}' references missing scene asset path '{scenePath}'.";
            return false;
        }

        private static bool ContainsLevel(LevelSequence levelSequence, LevelDefinition level)
        {
            foreach (LevelDefinition sequenceLevel in levelSequence.levels)
            {
                if (sequenceLevel == level) return true;
            }

            return false;
        }
    }

    public sealed class LevelSequenceBuildSettingsReport
    {
        public readonly List<string> missingScenes = new();
        public readonly List<string> disabledScenes = new();
        public readonly List<LevelSequenceBuildSettingsInvalidLevel> invalidLevels = new();
        public LevelDefinition effectiveBuildStartLevel;
        public string effectiveBuildStartScenePath;
        public bool usesBuildStartOverride;
        public bool isFirstSceneOutOfSync;

        public bool isSynced => invalidLevels.Count == 0 && !hasFixableIssues;
        public bool hasFixableIssues => isFirstSceneOutOfSync || missingScenes.Count > 0 || disabledScenes.Count > 0;
    }

    public sealed class LevelSequenceBuildSettingsInvalidLevel
    {
        public LevelSequenceBuildSettingsInvalidLevel(int index, LevelDefinition level, string reason)
        {
            this.index = index;
            this.level = level;
            this.reason = reason;
        }

        public readonly int index;
        public readonly LevelDefinition level;
        public readonly string reason;

        public override string ToString()
        {
            string levelName = level != null ? level.name : "null";
            string indexLabel = index >= 0 ? $"Entry {index + 1}" : "Entry unknown";
            return $"{indexLabel} ({levelName}): {reason}";
        }
    }

    public readonly struct LevelSequenceBuildSettingsSyncResult
    {
        public LevelSequenceBuildSettingsSyncResult(int addedCount, int enabledCount, int skippedInvalidLevelCount, int movedStartSceneCount)
        {
            this.addedCount = addedCount;
            this.enabledCount = enabledCount;
            this.skippedInvalidLevelCount = skippedInvalidLevelCount;
            this.movedStartSceneCount = movedStartSceneCount;
        }

        public readonly int addedCount;
        public readonly int enabledCount;
        public readonly int skippedInvalidLevelCount;
        public readonly int movedStartSceneCount;
    }
}
