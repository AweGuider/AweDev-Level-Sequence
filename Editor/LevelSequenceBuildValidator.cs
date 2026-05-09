using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AweDev.LevelSequence.Editor
{
    public class LevelSequenceBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!LevelSequenceEditorSettings.blockBuilds) return;

            if (!LevelSequenceEditorSettings.TryGetActiveLevelSequence(out LevelSequence levelSequence, out string errorMessage))
            {
                throw new BuildFailedException($"[LevelSequenceBuildValidator] {errorMessage}");
            }

            List<DuplicateLevelIdGroup> duplicateGroups = LevelSequenceValidator.FindDuplicateLevelIds(levelSequence);
            if (duplicateGroups.Count > 0)
            {
                throw new BuildFailedException(
                    $"[LevelSequenceBuildValidator] Duplicate level IDs found in active Level Sequence: {LevelSequenceValidator.FormatDuplicateIds(duplicateGroups)}.");
            }

            LevelSequenceBuildSettingsReport buildSettingsReport = LevelSequenceBuildSettingsSync.BuildReport(levelSequence);
            if (buildSettingsReport.invalidLevels.Count > 0)
            {
                throw new BuildFailedException(
                    $"[LevelSequenceBuildValidator] Active Level Sequence contains invalid scene references:\n{LevelSequenceBuildSettingsSync.FormatInvalidLevels(buildSettingsReport)}");
            }

            if (!buildSettingsReport.hasFixableIssues) return;

            string message = $"Active Level Sequence is not synced with Build Settings. {LevelSequenceBuildSettingsSync.FormatSummary(buildSettingsReport)}";

            if (Application.isBatchMode)
            {
                throw new BuildFailedException($"[LevelSequenceBuildValidator] {message}");
            }

            Debug.LogWarning($"[LevelSequenceBuildValidator] {message}");
        }
    }
}
