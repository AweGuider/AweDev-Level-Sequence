using System;
using System.IO;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AweDev.LevelSequence
{
    public sealed class UnitySceneLoader : ILevelSceneLoader
    {
        private enum SceneBuildSettingsStatus
        {
            Enabled,
            Disabled,
            Missing
        }

        public bool TryLoadLevel(LevelDefinition level, out string failureReason)
        {
            if (level == null)
            {
                failureReason = "Level is missing.";
                return false;
            }

            string sceneToLoad = level.sceneName;
            if (string.IsNullOrWhiteSpace(sceneToLoad))
            {
                failureReason = $"Level '{level.name}' has an empty scene name.";
                return false;
            }

            SceneBuildSettingsStatus buildSettingsStatus = GetSceneBuildSettingsStatus(sceneToLoad);
            if (buildSettingsStatus == SceneBuildSettingsStatus.Missing)
            {
                failureReason = $"Level '{level.name}' references scene '{sceneToLoad}', which is not in Build Settings.";
                return false;
            }

            if (buildSettingsStatus == SceneBuildSettingsStatus.Disabled)
            {
                failureReason = $"Level '{level.name}' references scene '{sceneToLoad}', which is disabled in Build Settings.";
                return false;
            }

            SceneManager.LoadSceneAsync(sceneToLoad);
            failureReason = null;
            return true;
        }

        private static SceneBuildSettingsStatus GetSceneBuildSettingsStatus(string sceneName)
        {
#if UNITY_EDITOR
            bool foundDisabledScene = false;

            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (!SceneNameMatchesPath(sceneName, buildScene.path)) continue;

                if (buildScene.enabled) return SceneBuildSettingsStatus.Enabled;

                foundDisabledScene = true;
            }

            return foundDisabledScene ? SceneBuildSettingsStatus.Disabled : SceneBuildSettingsStatus.Missing;
#else
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string buildScenePath = SceneUtility.GetScenePathByBuildIndex(i);

                if (SceneNameMatchesPath(sceneName, buildScenePath))
                {
                    return SceneBuildSettingsStatus.Enabled;
                }
            }

            return SceneBuildSettingsStatus.Missing;
#endif
        }

        private static bool SceneNameMatchesPath(string sceneName, string scenePath)
        {
            return string.Equals(Path.GetFileNameWithoutExtension(scenePath), sceneName, StringComparison.Ordinal);
        }
    }
}
