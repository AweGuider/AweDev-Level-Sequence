using System.IO;
using UnityEditor;
using UnityEngine;

namespace AweDev.LevelSequence.Editor
{
    public static class LevelSequenceEditorSettings
    {
        private const string ActiveSequenceGuidKey = "AweDev.LevelSequence.ActiveSequenceGuid";
        private const string BlockBuildsKey = "AweDev.LevelSequence.BlockBuilds";
        private const string PromptBuildSyncKey = "AweDev.LevelSequence.PromptBuildSync";
        private const string UseProductNameForDefaultCaptureFolderKey = "AweDev.LevelSequence.UseProductNameForDefaultCaptureFolder";
        private const string ReferenceImageOutputFolderKey = "AweDev.LevelSequence.ReferenceImageOutputFolder";
        private const string ReferenceImageFilenameFormatKey = "AweDev.LevelSequence.ReferenceImageFilenameFormat";
        private const string ReferenceImageCaptureWidthKey = "AweDev.LevelSequence.ReferenceImageCaptureWidth";
        private const string ReferenceImageCaptureHeightKey = "AweDev.LevelSequence.ReferenceImageCaptureHeight";
        private const string DefaultReferenceImageFilenameFormat = "{levelId}_reference";
        private const int DefaultReferenceImageCaptureWidth = 1024;
        private const int DefaultReferenceImageCaptureHeight = 576;
        private const int MinimumReferenceImageCaptureSize = 16;

        public static bool blockBuilds
        {
            get => EditorPrefs.GetBool(BlockBuildsKey, false);
            set => EditorPrefs.SetBool(BlockBuildsKey, value);
        }

        public static bool promptBuildSync
        {
            get => EditorPrefs.GetBool(PromptBuildSyncKey, false);
            set => EditorPrefs.SetBool(PromptBuildSyncKey, value);
        }

        public static bool useProductNameForDefaultCaptureFolder
        {
            get => EditorPrefs.GetBool(UseProductNameForDefaultCaptureFolderKey, false);
            set => EditorPrefs.SetBool(UseProductNameForDefaultCaptureFolderKey, value);
        }

        public static string defaultReferenceImageOutputFolder
        {
            get
            {
                string sourceName = useProductNameForDefaultCaptureFolder
                    ? Application.productName
                    : new DirectoryInfo(Application.dataPath).Parent?.Name;
                string folderName = SanitizePathSegment(sourceName);
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    folderName = "Project";
                }

                return $"Assets/{folderName}/Level/Reference Images";
            }
        }

        public static string referenceImageOutputFolder
        {
            get
            {
                string value = EditorPrefs.GetString(ReferenceImageOutputFolderKey, string.Empty);
                return string.IsNullOrWhiteSpace(value)
                    ? defaultReferenceImageOutputFolder
                    : NormalizeAssetFolderPath(value);
            }
            set
            {
                value = NormalizeAssetFolderPath(value);
                if (string.IsNullOrWhiteSpace(value) || value == defaultReferenceImageOutputFolder)
                {
                    EditorPrefs.DeleteKey(ReferenceImageOutputFolderKey);
                    return;
                }

                EditorPrefs.SetString(ReferenceImageOutputFolderKey, value);
            }
        }

        public static string referenceImageFilenameFormat
        {
            get
            {
                string value = EditorPrefs.GetString(ReferenceImageFilenameFormatKey, string.Empty);
                return string.IsNullOrWhiteSpace(value) ? DefaultReferenceImageFilenameFormat : value.Trim();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Trim() == DefaultReferenceImageFilenameFormat)
                {
                    EditorPrefs.DeleteKey(ReferenceImageFilenameFormatKey);
                    return;
                }

                EditorPrefs.SetString(ReferenceImageFilenameFormatKey, value.Trim());
            }
        }

        public static int referenceImageCaptureWidth
        {
            get => Mathf.Max(MinimumReferenceImageCaptureSize, EditorPrefs.GetInt(ReferenceImageCaptureWidthKey, DefaultReferenceImageCaptureWidth));
            set => SetReferenceImageCaptureSize(ReferenceImageCaptureWidthKey, value, DefaultReferenceImageCaptureWidth);
        }

        public static int referenceImageCaptureHeight
        {
            get => Mathf.Max(MinimumReferenceImageCaptureSize, EditorPrefs.GetInt(ReferenceImageCaptureHeightKey, DefaultReferenceImageCaptureHeight));
            set => SetReferenceImageCaptureSize(ReferenceImageCaptureHeightKey, value, DefaultReferenceImageCaptureHeight);
        }

        public static LevelSequence activeSequence
        {
            get
            {
                string guid = EditorPrefs.GetString(ActiveSequenceGuidKey, string.Empty);
                if (string.IsNullOrWhiteSpace(guid)) return null;

                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                return string.IsNullOrWhiteSpace(assetPath)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<LevelSequence>(assetPath);
            }
            set
            {
                if (value == null)
                {
                    EditorPrefs.DeleteKey(ActiveSequenceGuidKey);
                    return;
                }

                string assetPath = AssetDatabase.GetAssetPath(value);
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                EditorPrefs.SetString(ActiveSequenceGuidKey, guid);
            }
        }

        public static bool TryGetActiveLevelSequence(out LevelSequence levelSequence, out string errorMessage)
        {
            levelSequence = activeSequence;
            if (levelSequence != null)
            {
                errorMessage = null;
                return true;
            }

            errorMessage = "No active Level Sequence is configured in Project Settings > AweDev > Level Sequence.";
            return false;
        }

        public static string NormalizeAssetFolderPath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Trim().Replace('\\', '/').TrimEnd('/');
        }

        private static void SetReferenceImageCaptureSize(string key, int value, int defaultValue)
        {
            value = Mathf.Max(MinimumReferenceImageCaptureSize, value);
            if (value == defaultValue)
            {
                EditorPrefs.DeleteKey(key);
                return;
            }

            EditorPrefs.SetInt(key, value);
        }

        private static string SanitizePathSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            char[] characters = value.Trim().ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                if (System.Array.IndexOf(invalidChars, characters[i]) >= 0)
                {
                    characters[i] = '_';
                }
            }

            return new string(characters);
        }
    }
}
